# Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
# SPDX-License-Identifier: Apache-2.0

import os
import json


def handler(event, context):
    """
    Handles requests to start games from the game client.
     By design of the deployment scenario, this function is stubbed to always return a 501 (Not implemented) error.
     However, the lambda echoes the lambda environment variable as well as the player latency (if any) in the
     CloudWatch logs to provide an example of how they are retrieved in the Lambda function.
    :param event: lambda event, contains the region to player latency mapping in `regionToLatencyMapping` key, as well
     as the playerId from the Cognito id tokens.
    :param context: lambda context, not used by this function
    :return:
     - 500 (Internal Error) if error occurred when processing the matchmaking request
     - 501 (Not Implemented) when the function succeeds
    """

    player_id = event["requestContext"]["authorizer"]["claims"]["sub"]
    print(f'Handling start game request. PlayerId: {player_id}')

    game_name = os.environ['GameName']
    print(f"Lambda environment variable GameName: {game_name}. Event: {event.get('body')}")

    region_to_latency_mapping = get_region_to_latency_mapping(event)
    if region_to_latency_mapping:
        print(f"Region to latency mapping: {region_to_latency_mapping}")
    else:
        print("No regionToLatencyMapping mapping provided")

    return {
        'body': json.dumps(region_to_latency_mapping),
        'headers': {
            'Content-Type': 'text/plain'
        },
        'statusCode': 501  # Not Implemented
    }


def get_region_to_latency_mapping(event):
    request_body = event.get("body")
    if not request_body:
        return None

    try:
        request_body_json = json.loads(request_body)
    except ValueError:
        print(f"Error parsing request body: {request_body}")
        return None

    if request_body_json and request_body_json.get('regionToLatencyMapping'):
        return request_body_json.get('regionToLatencyMapping')
