#!/usr/bin/env bash

MANIFEST_FILE='Sunshine/Properties/AndroidManifest.xml'
PACKAGENAME=`grep "package" $MANIFEST_FILE | sed 's/.*package="//;s/".*//'`
NEWNAME=$PACKAGENAME.DEBUG

if [ "$APPCENTER_BRANCH" == "master" ];
then

echo "Package name is $PACKAGENAME and new is $NEWNAME";
sed -i 's/package=*"'$PACKAGENAME'"/package="'$NEWNAME'"/g' $MANIFEST_FILE

fi
