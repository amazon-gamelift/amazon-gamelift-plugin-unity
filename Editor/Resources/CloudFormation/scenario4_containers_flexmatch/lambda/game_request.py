# Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
# SPDX-License-Identifier: MIT-0

import boto3
from boto3.dynamodb.conditions import Key
import time
import os
import json

DEFAULT_TTL_IN_SECONDS = 10 * 60  # 10 minutes
MATCHMAKING_STARTED_STATUS = 'MatchmakingStarted'
MATCHMAKING_SUCCEEDED_STATUS = 'MatchmakingSucceeded'
MATCHMAKING_TIMED_OUT_STATUS = 'MatchmakingTimedOut'
MATCHMAKING_CANCELLED_STATUS = 'MatchmakingCancelled'
MATCHMAKING_FAILED_STATUS = 'MatchmakingFailed'


def handler(event, context):
    """
    Handles requests to start games from the game client.
     This function records the game request from the client in the MatchmakingRequest table and calls
     GameLift to start matchmaking.
    :param event: lambda event, contains the region to player latency mapping in `regionToLatencyMapping` key, as well
     as the player information from the Cognito id tokens.
    :param context: lambda context, not used by this function
    :return:
     - 202 (Accepted) if the matchmaking request is accepted and is now being processed
     - 409 (Conflict) if the another matchmaking request is in progress
     - 500 (Internal Error) if error occurred when calling GameLift to start matchmaking
    """
    player_id = event["requestContext"]["authorizer"]["claims"]["sub"]
    start_time = round(time.time())
    print(f'Handling start game request. PlayerId: {player_id}, StartTime: {start_time}')

    region_to_latency_mapping = get_region_to_latency_mapping(event)
    if region_to_latency_mapping:
        print(f"Region to latency mapping: {region_to_latency_mapping}")
    else:
        print("No regionToLatencyMapping mapping provided")

    matchmaking_request_table_name = os.environ['MatchmakingRequestTableName']
    team_name = os.environ['TeamName']
    matchmaking_configuration_name = os.environ['MatchmakingConfigurationName']

    dynamodb = boto3.resource('dynamodb')
    matchmaking_request_table = dynamodb.Table(matchmaking_request_table_name)

    gamelift = boto3.client('gamelift')

    matchmaking_requests = matchmaking_request_table.query(
        KeyConditionExpression=Key('PlayerId').eq(player_id),
        ScanIndexForward=False
    )

    if matchmaking_requests['Count'] > 0 \
            and not is_matchmaking_request_terminal(matchmaking_requests['Items'][0]):
        # A existing matchmaking request in progress
        return {
            'headers': {
                'Content-Type': 'text/plain'
            },
            'statusCode': 409  # Conflict
        }

    try:
        player = {
            'PlayerId': player_id,
            'Team': team_name
        }
        if region_to_latency_mapping:
            player['LatencyInMs'] = region_to_latency_mapping

        start_matchmaking_request = {
            "ConfigurationName": matchmaking_configuration_name,
            "Players": [player]
        }
        print(f"Starting matchmaking in GameLift. Request: {start_matchmaking_request}")
        start_matchmaking_result = gamelift.start_matchmaking(**start_matchmaking_request)

        ticket_id = start_matchmaking_result['MatchmakingTicket']['TicketId']
        ticket_status = MATCHMAKING_STARTED_STATUS

        matchmaking_request_table.put_item(
            Item={
                'PlayerId': player_id,
                'StartTime': start_time,
                'LastUpdatedTime': start_time,
                'ExpirationTime': start_time + DEFAULT_TTL_IN_SECONDS,
                'TicketStatus': ticket_status,
                'TicketId': ticket_id
            }
        )

        return {
            # Matchmaking request enqueued
            'headers': {
                'Content-Type': 'text/plain'
            },
            'statusCode': 202
        }
    except Exception as ex:
        print(f'Error occurred when calling GameLift to start matchmaking. Exception: {ex}')
        return {
            # Error occurred when enqueuing matchmaking request
            'headers': {
                'Content-Type': 'text/plain'
            },
            'statusCode': 500
        }


def is_matchmaking_request_terminal(matchmaking_request):
    return matchmaking_request['TicketStatus'] in [
        MATCHMAKING_SUCCEEDED_STATUS,
        MATCHMAKING_TIMED_OUT_STATUS,
        MATCHMAKING_CANCELLED_STATUS,
        MATCHMAKING_FAILED_STATUS
    ]


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
