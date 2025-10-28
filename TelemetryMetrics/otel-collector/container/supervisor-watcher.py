#!/usr/bin/python
# Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
# SPDX-License-Identifier: Apache-2.0

import sys
import os
import logging
import subprocess
import time

from supervisor.childutils import listener

def main(args):
    logging.basicConfig(stream=sys.stderr, level=logging.DEBUG, format='%(asctime)s %(levelname)s %(filename)s: %(message)s')
    logger = logging.getLogger("supervisord-watchdog")
    debug_mode = True if 'DEBUG' in os.environ else False

    logger.info("Listening for events...")
    while True:
        headers, body = listener.wait(sys.stdin, sys.stdout)
        body = dict([pair.split(":") for pair in body.split(" ")])

        if debug_mode: continue

        try:
            if headers["eventname"] == "PROCESS_STATE_FATAL" or headers["eventname"] == "PROCESS_STATE_EXITED":
                if not args or body["processname"] in args:
                    logger.info("[WATCHER] Game not running or has exited. Waiting 10 seconds before shutting down supervisord...")
                    time.sleep(10)
                    logger.info("[WATCHER] Shutting down supervisord...")
                    subprocess.call(["/usr/local/bin/supervisorctl", "shutdown"], stdout=sys.stderr)

        except Exception as e:
            logger.critical("Unexpected Exception: %s", str(e))
            listener.fail(sys.stdout)
            exit(1)
        else:
            listener.ok(sys.stdout)

if __name__ == '__main__':
    main(sys.argv[1:])