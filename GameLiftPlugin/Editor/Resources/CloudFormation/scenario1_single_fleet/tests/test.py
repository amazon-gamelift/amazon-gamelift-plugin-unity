# Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
# SPDX-License-Identifier: MIT-0

import boto3
import requests
import json
import time

GAME_NAME = '<REPLACE_WITH_YOUR_GAME_NAME>'
REGION = '<REPLACE_WITH_YOUR_REGION>'

USER_POOL_NAME = GAME_NAME + 'UserPool'
USER_POOL_CLIENT_NAME = GAME_NAME + 'UserPoolClient'
USERNAME = 'testuser@example.com'
PASSWORD = 'TestPassw0rd.'
REST_API_NAME = GAME_NAME + 'RestApi'
REST_API_STAGE = 'v1'
GAME_REQUEST_PATH = 'start_game'
RESULTS_REQUEST_PATH = 'get_game_connection'
cognito_idp = boto3.client('cognito-idp', region_name=REGION)
apig = boto3.client('apigateway', region_name=REGION)
REGION_TO_LATENCY_MAPPING = {
    "regionToLatencyMapping": {
        "us-west-2": 50,
        "us-east-1": 100,
        "eu-west-1": 150,
        "ap-northeast-1": 300
    }
}
GAME_REQUEST_PAYLOAD = json.dumps(REGION_TO_LATENCY_MAPPING)


def main():
    user_pool = find_user_pool(USER_POOL_NAME)
    user_pool_id = user_pool['Id']
    print("User Pool Id:", user_pool_id)

    user_pool_client = find_user_pool_client(user_pool_id, USER_POOL_CLIENT_NAME)
    user_pool_client_id = user_pool_client['ClientId']
    print("User Pool Client Id:", user_pool_client_id)

    try:
        cognito_idp.sign_up(
            ClientId=user_pool_client_id,
            Username=USERNAME,
            Password=PASSWORD,
        )

        print("Created user:", USERNAME)

        cognito_idp.admin_confirm_sign_up(
            UserPoolId=user_pool_id,
            Username=USERNAME,
        )

        init_auth_result = cognito_idp.initiate_auth(
            AuthFlow='USER_PASSWORD_AUTH',
            AuthParameters={
                'USERNAME': USERNAME,
                'PASSWORD': PASSWORD,
            },
            ClientId=user_pool_client_id
        )

        assert init_auth_result['ResponseMetadata']['HTTPStatusCode'] == 200, "Unsuccessful init_auth"
        print("Authenticated via username and password")

        id_token = init_auth_result['AuthenticationResult']['IdToken']
        headers = {
            'Auth': id_token
        }
        results_request_url = get_rest_api_endpoint(REST_API_NAME, REGION, REST_API_STAGE, RESULTS_REQUEST_PATH)
        game_request_url = get_rest_api_endpoint(REST_API_NAME, REGION, REST_API_STAGE, GAME_REQUEST_PATH)

        results_request_response = requests.post(url=results_request_url, headers=headers)
        assert results_request_response.status_code == 204 or results_request_response.status_code == 200, \
            "Expect 'POST /get_game_info' status code to be 200 (Success) or 204 (No Content). Actual: " \
            f"{str(results_request_response.status_code)}"
        print("Verified mock ResultsRequest response", results_request_response)

        game_request_response = requests.post(url=game_request_url, headers=headers, data=GAME_REQUEST_PAYLOAD)
        assert game_request_response.status_code == 202, "Expect 'POST /start_game' status code to be 202 (Accepted)/ Actual: " \
            f"{str(results_request_response.status_code)}"
        print("Verified lambda GameRequest response", game_request_response)

        print("Waiting for game session to be created...")
        time.sleep(10)

        results_request_response = requests.post(url=results_request_url, headers=headers)
        assert results_request_response.status_code == 200, "Expect 'POST /get_game_info' status code to be 200 (Success). Actual: " \
            f"{str(results_request_response.status_code)}"
        print("Verified lambda ResultsRequest response", results_request_response)

        game_connection_info = json.loads(results_request_response.content)
        print(f"Received game connection info: {game_connection_info}")
        assert game_connection_info['IpAddress'] != ''
        assert game_connection_info['Port'] > 0
        assert "us-west-2" in game_connection_info['DnsName'], \
            f"Expect {game_connection_info['DnsName']} to contain 'us-west-2'"
        assert "psess-" in game_connection_info['PlayerSessionId'], \
            f"Expect {game_connection_info['PlayerSessionId']} to contain 'psess-'"
        assert "us-west-2" in game_connection_info['GameSessionArn'], \
            f"Expect {game_connection_info['GameSessionArn']} to contain 'us-west-2'"
        print("Verified game connection info:", game_connection_info)

    finally:
        cognito_idp.admin_delete_user(
            UserPoolId=user_pool_id,
            Username=USERNAME,
        )

        print("Deleted user:", USERNAME)

        print("Test Succeeded!")


def find_user_pool(user_pool_name):
    print("Finding user pool:", user_pool_name)
    result = cognito_idp.list_user_pools(MaxResults=50)
    pools = result['UserPools']
    return next(x for x in pools if x['Name'] == user_pool_name)


def find_user_pool_client(user_pool_id, user_pool_client_name):
    print("Finding user pool client:", user_pool_client_name)
    results = cognito_idp.list_user_pool_clients(UserPoolId=user_pool_id)
    clients = results['UserPoolClients']
    return next(x for x in clients if x['ClientName'] == user_pool_client_name)


def find_rest_api(rest_api_name):
    print("Finding rest api:", rest_api_name)
    results = apig.get_rest_apis()
    rest_apis = results['items']
    return next(x for x in rest_apis if x['name'] == rest_api_name)


def get_rest_api_endpoint(rest_api_name, region, stage, path):
    print("Getting rest api endpoint", rest_api_name)
    rest_api = find_rest_api(rest_api_name)
    rest_api_id = rest_api['id']
    return f'https://{rest_api_id}.execute-api.{region}.amazonaws.com/{stage}/{path}'


if __name__ == '__main__':
    main()
