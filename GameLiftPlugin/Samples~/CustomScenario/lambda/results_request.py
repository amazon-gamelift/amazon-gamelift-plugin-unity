# Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
# SPDX-License-Identifier: Apache-2.0

import os


def handler(event, context):
    """
    Handles requests to describe the game session connection information after a StartGame request.
     This function always returns 501 (Not Implemented) because this deployment scenario does not include
     any capacity for hosting game sessions.
    :param event: lambda event, contains the region to player latency mapping in `regionToLatencyMapping` key, as well
     as the player information from the Cognito id tokens.
    :param context: lambda context, not used by this function
    :return:
     - 500 (Internal Error) if error occurred when processing the matchmaking request
     - 501 (Not Implemented) when the function succeeds
    """

    player_id = event["requestContext"]["authorizer"]["claims"]["sub"]
    print(f'Handling request result request. PlayerId: {player_id}')

    game_name = os.environ['GameName']
    print(f"Lambda environment variable GameName: {game_name}")
    return {
        'headers': {
            'Content-Type': 'text/plain'
        },
        'statusCode': 501  # Not Implemented
    }
