# Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
# SPDX-License-Identifier: MIT-0

import boto3
import os
import json
from boto3.dynamodb.conditions import Key

MATCHMAKING_STARTED_STATUS = 'MatchmakingStarted'
MATCHMAKING_SUCCEEDED_STATUS = 'MatchmakingSucceeded'


def handler(event, context):
    """
    Handles requests to describe the game session connection information after a StartGame request.
     This function will look up the MatchmakingRequest table to find a the latest matchmaking request
     by the player, and return its game connection information if any.
    :param event: lambda event, contains the region to player latency mapping in `regionToLatencyMapping` key, as well
     as the player information from the Cognito id tokens.
    :param context: lambda context, not used by this function
    :return:
     - 200 (OK) if the game connection is ready, along with server info: "IpAddress", "Port", "DnsName"
     - 204 (No Content) if the requested game is still in progress of matchmaking
     - 404 (Not Found) if no game has been started by the player, or if all started game were expired
     - 500 (Internal Error) if errors occurred during matchmaking or placement
    """
    player_id = event["requestContext"]["authorizer"]["claims"]["sub"]
    print(f'Handling request result request. PlayerId: {player_id}')

    matchmaking_request_table_name = os.environ['MatchmakingRequestTableName']

    dynamodb = boto3.resource('dynamodb')
    matchmaking_request_table = dynamodb.Table(matchmaking_request_table_name)

    matchmaking_requests = matchmaking_request_table.query(
        KeyConditionExpression=Key('PlayerId').eq(player_id),
        ScanIndexForward=False
    )

    if matchmaking_requests['Count'] <= 0:
        return {
            'headers': {
                'Content-Type': 'text/plain'
            },
            'statusCode': 404
        }

    latest_matchmaking_request = matchmaking_requests['Items'][0]

    print(f'Current Matchmaking Request: {latest_matchmaking_request}')

    matchmaking_request_status = latest_matchmaking_request['TicketStatus']

    if matchmaking_request_status == MATCHMAKING_STARTED_STATUS:
        # still waiting for ticket to be processed
        return {
            'headers': {
                'Content-Type': 'text/plain'
            },
            'statusCode': 204
        }
    elif matchmaking_request_status == MATCHMAKING_SUCCEEDED_STATUS:
        game_session_connection_info = \
            dict((k, latest_matchmaking_request[k]) for k in ('IpAddress', 'Port', 'DnsName', 'PlayerSessionId', 'GameSessionArn'))
        print(game_session_connection_info)
        return {
            'body': json.dumps(game_session_connection_info),
            'headers': {
                'Content-Type': 'text/plain'
            },
            'statusCode': 200
        }
    else:
        # We count MatchmakingCancelled as internal error also because cancelling placement requests is not
        # in the current implementation, so it should never happen.
        print(f'Received non-successful terminal status {matchmaking_request_status}, responding with 500 error.')
        return {
            'headers': {
                'Content-Type': 'text/plain'
            },
            'statusCode': 500
        }
