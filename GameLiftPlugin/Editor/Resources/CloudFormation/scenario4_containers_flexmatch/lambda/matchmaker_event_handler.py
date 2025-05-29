# Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
# SPDX-License-Identifier: MIT-0

import boto3
from boto3.dynamodb.conditions import Key
import os
import json
import time

MATCHMAKING_STARTED_STATUS = 'MatchmakingStarted'
MATCHMAKING_SUCCEEDED_STATUS = 'MatchmakingSucceeded'
MATCHMAKING_TIMED_OUT_STATUS = 'MatchmakingTimedOut'
MATCHMAKING_CANCELLED_STATUS = 'MatchmakingCancelled'
MATCHMAKING_FAILED_STATUS = 'MatchmakingFailed'


def handler(event, context):
    """
    Handles game session event from GameLift FlexMatch. This function parses the event messages and updates all
     related requests in the the MatchmakingPlacement DynamoDB table,
     which will be looked up to fulfill game client's Results Requests.
    :param event: lambda event containing game session event from GameLift queue
    :param context: lambda context, not used by this function
    :return: None
    """
    lambda_start_time = round(time.time())
    message = json.loads(event['Records'][0]['Sns']['Message'])
    print(f'Handling FlexMatch event. StartTime: {lambda_start_time}. Message: {message}')

    status_type = message['detail']['type']

    if status_type not in [MATCHMAKING_SUCCEEDED_STATUS, MATCHMAKING_TIMED_OUT_STATUS, MATCHMAKING_CANCELLED_STATUS,
                           MATCHMAKING_FAILED_STATUS]:
        print(f'Received non-terminal status type: {status_type}. Skip processing.')
        return

    tickets = message['detail']['tickets']
    ip_address = message['detail']['gameSessionInfo'].get('ipAddress')
    dns_name = message['detail']['gameSessionInfo'].get('dnsName')
    port = str(message['detail']['gameSessionInfo'].get('port'))
    game_session_arn = str(message['detail']['gameSessionInfo'].get('gameSessionArn'))
    players = message['detail']['gameSessionInfo']['players']
    players_map = {player.get('playerId'):player.get('playerSessionId') for player in players}

    ticket_id_index_name = os.environ['TicketIdIndexName']

    matchmaking_request_table_name = os.environ['MatchmakingRequestTableName']
    dynamodb = boto3.resource('dynamodb')
    matchmaking_request_table = dynamodb.Table(matchmaking_request_table_name)

    for ticket in tickets:
        ticket_id = ticket['ticketId']
        matchmaking_requests = matchmaking_request_table.query(
            IndexName=ticket_id_index_name,
            KeyConditionExpression=Key('TicketId').eq(ticket_id)
        )

        if matchmaking_requests['Count'] <= 0:
            print(f"Cannot find matchmaking request with ticket id: {ticket_id}")
            continue

        matchmaking_request_status = matchmaking_requests['Items'][0]['TicketStatus']
        player_id = matchmaking_requests['Items'][0]['PlayerId']
        player_session_id = players_map.get(player_id)
        print(f'Processing Ticket: {ticket_id}, PlayerId: {player_id}, PlayerSessionId: {player_session_id}')
        matchmaking_request_start_time = matchmaking_requests['Items'][0]['StartTime']

        if matchmaking_request_status != MATCHMAKING_STARTED_STATUS:
            print(f"Unexpected TicketStatus on matchmaking request. Expected: 'MatchmakingStarted'. "
                  f"Found: {matchmaking_request_status}")
            continue

        attribute_updates = {
            'TicketStatus': {
                'Value': status_type
            },
            'LastUpdatedTime': {
                'Value': lambda_start_time
            }
        }

        if status_type == MATCHMAKING_SUCCEEDED_STATUS:
            attribute_updates.update({
                'IpAddress': {
                    'Value': ip_address
                },
                'DnsName': {
                    'Value': dns_name
                },
                'Port': {
                    'Value': port
                },
                'GameSessionArn': {
                    'Value': game_session_arn
                },
                'PlayerSessionId': {
                    'Value': player_session_id
                }
            })

        matchmaking_request_table.update_item(
            Key={
                'PlayerId': player_id,
                'StartTime': matchmaking_request_start_time
            },
            AttributeUpdates=attribute_updates
        )
