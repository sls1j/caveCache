#!/bin/bash

mongodump
DUMP_DIR=mongo-dump-`date`
mv dump $DUMP_DIR
tar -avcf $DUMP_DIR.tgz $DUMP_DIR
rm -rf $DUMP_DIR