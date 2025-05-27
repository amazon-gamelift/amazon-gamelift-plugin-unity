# Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
# SPDX-License-Identifier: MIT-0

import boto3
from boto3.dynamodb.conditions import Attr, And
from botocore.exceptions import ClientError
import os
import time

NON_TERMINAL_REQUEST_QUERY_LIMIT = 50
MATCHMAKING_STARTED_STATUS = 'MatchmakingStarted'
MATCHMAKING_SUCCEEDED_STATUS = 'MatchmakingSucceeded'
MATCHMAKING_TIMED_OUT_STATUS = 'MatchmakingTimedOut'
MATCHMAKING_CANCELLED_STATUS = 'MatchmakingCancelled'
MATCHMAKING_FAILED_STATUS = 'MatchmakingFailed'
MAX_PARTITION_SIZE = 10
MIN_TIME_ELAPSED_BEFORE_UPDATE_IN_SECONDS = 30


def handler(event, context):
    """
    Finds all non-terminal matchmaking requests and poll for their status. It is recommended by GameLift
    to regularly poll for ticket status at a low rate.
    See: https://docs.aws.amazon.com/gamelift/latest/flexmatchguide/match-client.html#match-client-track

    :param event: lambda event, not used by this function
    :param context: lambda context, not used by this function
    :return: None
    """
    lambda_start_time = round(time.time())

    print(f"Polling non-terminal matchmaking tickets. Lambda start time: {lambda_start_time}")

    matchmaking_request_table_name = os.environ['MatchmakingRequestTableName']

    dynamodb = boto3.resource('dynamodb')
    matchmaking_request_table = dynamodb.Table(matchmaking_request_table_name)

    gamelift = boto3.client('gamelift')

    matchmaking_requests = matchmaking_request_table.scan(
        Limit=NON_TERMINAL_REQUEST_QUERY_LIMIT,
        FilterExpression=And(Attr('TicketStatus').eq(MATCHMAKING_STARTED_STATUS),
                             Attr('LastUpdatedTime').lt(lambda_start_time - MIN_TIME_ELAPSED_BEFORE_UPDATE_IN_SECONDS))
    )

    if matchmaking_requests['Count'] <= 0:
        print("No non-terminal matchmaking requests found")

    for matchmaking_requests in partition(matchmaking_requests['Items'], MAX_PARTITION_SIZE):
        ticket_id_to_request_mapping = {request['TicketId']: request for request in matchmaking_requests}
        describe_matchmaking_result = gamelift.describe_matchmaking(
            TicketIds=list(ticket_id_to_request_mapping.keys())
        )
        ticket_list = describe_matchmaking_result['TicketList']
        if len(ticket_list) != len(matchmaking_requests):
            print(f"Resulting TicketList length: {len(ticket_list)} from DescribeMatchmaking "
                  f"does not match the request size: {len(matchmaking_requests)}")
        for ticket in ticket_list:
            ticket_id = ticket['TicketId']
            ticket_status = ticket['Status']
            matchmaking_request_status = to_matchmaking_request_status(ticket_status)
            matchmaking_request = ticket_id_to_request_mapping[ticket_id]
            player_id = matchmaking_request['PlayerId']
            start_time = matchmaking_request['StartTime']
            last_updated_time = matchmaking_request['LastUpdatedTime']
            try:
                attribute_updates = {
                    'LastUpdatedTime': {
                        'Value': lambda_start_time
                    }
                }
                if ticket_status in ['COMPLETED', 'FAILED', 'TIMED_OUT', 'CANCELLED']:
                    print(f'Ticket: {ticket_id} status was updated to {ticket_status}')
                    attribute_updates.update({
                        'TicketStatus': {
                            'Value': matchmaking_request_status
                        }
                    })
                    if ticket_status == 'COMPLETED':
                        # parse the playerSessionId
                        matched_player_sessions = ticket.get('GameSessionConnectionInfo', {}).get('MatchedPlayerSessions')
                        player_session_id = None
                        if matched_player_sessions is not None and len(matched_player_sessions) == 1:
                            player_session_id = matched_player_sessions[0].get('PlayerSessionId')

                        attribute_updates.update({
                            'IpAddress': {
                                'Value': ticket.get('GameSessionConnectionInfo', {}).get('IpAddress')
                            },
                            'DnsName': {
                                'Value': ticket.get('GameSessionConnectionInfo', {}).get('DnsName')
                            },
                            'Port': {
                                'Value': str(ticket.get('GameSessionConnectionInfo', {}).get('Port'))
                            },
                            'GameSessionArn': {
                                'Value': str(ticket.get('GameSessionConnectionInfo', {}).get('GameSessionArn'))
                            },
                            'PlayerSessionId': {
                                'Value' : str(player_session_id)
                            }
                        })
                else:
                    print(f'No updates to ticket: {ticket_id} compared to '
                          f'{lambda_start_time - last_updated_time} seconds ago')

                matchmaking_request_table.update_item(
                    Key={
                        'PlayerId': player_id,
                        'StartTime': start_time
                    },
                    AttributeUpdates=attribute_updates,
                    Expected={
                        'TicketStatus': {
                            'Value': MATCHMAKING_STARTED_STATUS,
                            'ComparisonOperator': 'EQ'
                        }
                    }
                )

            except ClientError as e:
                error_code = e.response['Error']['Code']
                if error_code == 'ConditionCheckFailedException':
                    print(f"Ticket: {ticket_id} status has been updated (likely by MatchMakerEventHandler). "
                          f"No change is made")
                    continue
                raise e


def partition(collection, n):
    """Yield successive n-sized partitions from collection."""
    for i in range(0, len(collection), n):
        yield collection[i:i + n]


def to_matchmaking_request_status(ticket_status):
    if ticket_status == 'COMPLETED':
        return MATCHMAKING_SUCCEEDED_STATUS
    if ticket_status == 'FAILED':
        return MATCHMAKING_FAILED_STATUS
    if ticket_status == 'TIMED_OUT':
        return MATCHMAKING_TIMED_OUT_STATUS
    if ticket_status == 'CANCELLED':
        return MATCHMAKING_CANCELLED_STATUS
