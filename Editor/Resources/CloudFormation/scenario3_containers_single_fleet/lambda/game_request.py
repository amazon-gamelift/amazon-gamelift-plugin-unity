# Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
# SPDX-License-Identifier: MIT-0

import boto3
import os
import json


def handler(event, context):
    """
    Handles requests to start games from the game client.
     This function first looks for any game session
    :param event: lambda event, contains the region to player latency mapping in `regionToLatencyMapping` key, as well
     as the player information from the Cognito id tokens.
    :param context: lambda context, not used by this function
    :return:
     - 202 (Accepted) if the matchmaking request is accepted and is now being processed
     - 409 (Conflict) if the another matchmaking request is in progress
     - 500 (Internal Error) if error occurred when processing the matchmaking request
    """

    gamelift = boto3.client('gamelift')
    fleet_alias = os.environ['FleetAlias']
    max_players_per_game = int(os.environ['MaxPlayersPerGame'])

    player_id = event["requestContext"]["authorizer"]["claims"]["sub"]
    print(f'Handling start game request. PlayerId: {player_id}')

    # NOTE: latency mapping is not used in this deployment scenario
    region_to_latency_mapping = get_region_to_latency_mapping(event)
    if region_to_latency_mapping:
        print(f"Region to latency mapping: {region_to_latency_mapping}")
    else:
        print("No regionToLatencyMapping mapping provided")

    if not has_viable_game_sessions(gamelift, fleet_alias):
        create_game_session(gamelift, fleet_alias, max_players_per_game)

    return {
        'headers': {
            'Content-Type': 'text/plain'
        },
        'statusCode': 202
    }


def has_viable_game_sessions(gamelift, fleet_alias):
    print(f"Checking for viable game sessions: {fleet_alias}")

    # NOTE: SortExpression="creationTimeMillis ASC" is not needed because we are looking for any viable game sessions,
    # hence the order does not matter.
    search_game_sessions_response = gamelift.search_game_sessions(
        AliasId=fleet_alias,
        FilterExpression="hasAvailablePlayerSessions=true",
    )
    return len(search_game_sessions_response['GameSessions']) != 0


def create_game_session(gamelift, fleet_alias, max_players_per_game):
    print(f"Creating game session: {fleet_alias}")
    gamelift.create_game_session(
        AliasId=fleet_alias,
        MaximumPlayerSessionCount=max_players_per_game,
    )


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
