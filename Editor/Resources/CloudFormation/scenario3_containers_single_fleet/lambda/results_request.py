# Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
# SPDX-License-Identifier: MIT-0

import boto3
import os
import json


def handler(event, context):
    """
    Handles requests to describe the game session connection information after a StartGame request.
     This function will look up the MatchmakingRequest table to find a pending matchmaking request by
     the player, and if it is QUEUED, look up the GameSessionPlacement table to find the game's
     connection information.
    :param event: lambda event, contains the region to player latency mapping in `regionToLatencyMapping` key, as well
     as the player information from the Cognito id tokens.
    :param context: lambda context, not used by this function
    :return:
     - 200 (OK) if the game connection is ready, along with server info: "IpAddress", "Port", "DnsName"
     - 204 (No Content) if the requested game is still in progress of matchmaking
     - 404 (Not Found) if no game has been started by the player, or if all started game were expired
     - 500 (Internal Error) if errors occurred during matchmaking or placement
    """

    gamelift = boto3.client('gamelift')
    fleet_alias = os.environ['FleetAlias']

    player_id = event["requestContext"]["authorizer"]["claims"]["sub"]
    print(f'Handling request result request. PlayerId: {player_id}')

    oldest_viable_game_session = get_oldest_viable_game_session(gamelift, fleet_alias)
    if oldest_viable_game_session:
        player_session = create_player_session(gamelift, oldest_viable_game_session['GameSessionId'], player_id)

        game_session_connection_info = dict((k, player_session[k]) for k in ('IpAddress', 'Port', 'DnsName', 'PlayerSessionId'))
        game_session_connection_info['GameSessionArn'] = player_session['GameSessionId']
        print(f"Connection Info: {game_session_connection_info}")

        return {
            'body': json.dumps(game_session_connection_info),
            'headers': {
                'Content-Type': 'text/plain'
            },
            'statusCode': 200
        }
    else:
        return {
            'headers': {
                'Content-Type': 'text/plain'
            },
            'statusCode': 204
        }


def get_oldest_viable_game_session(gamelift, fleet_alias):
    print("Checking for viable game sessions:", fleet_alias)
    search_game_sessions_response = gamelift.search_game_sessions(
        AliasId=fleet_alias,
        FilterExpression="hasAvailablePlayerSessions=true",
        SortExpression="creationTimeMillis ASC",
    )
    print(f"Received search game session response: {search_game_sessions_response}")
    return next(iter(search_game_sessions_response['GameSessions']), None)

def create_player_session(gamelift, game_session_id, player_id):
    print(f"Creating PlayerSession session on GameSession: {game_session_id}, PlayerId: {player_id}")
    create_player_session_response = gamelift.create_player_session(
        GameSessionId=game_session_id,
        PlayerId=player_id
    )
    print(f"Received create player session response: {create_player_session_response}")
    return create_player_session_response['PlayerSession']
