#!/usr/bin/env bash

MANIFEST_FILE='Properties/AndroidManifest.xml'
#PACKAGENAME=`grep "package" $MANIFEST_FILE | sed 's/.*package="//;s/".*//'`
#VERSIONNAME=`grep "versionName" $MANIFEST_FILE | sed 's/.*versionName="//;s/".*//'`
#NEWNAME=$PACKAGENAME.DEBUG
#GITCOUNT=$(git rev-list --all --count)
#NEWVERSIONNAME="$APP_VERSION.$GITCOUNT"

#if [ "$APPCENTER_BRANCH" == "master" ];
#echo "Package name is $PACKAGENAME and new is $NEWNAME";
#then
#echo "Old version name is $VERSIONNAME and new version name is $NEWVERSIONNAME"
#sed -i '' 's/package=*"'$PACKAGENAME'"/package="'$NEWNAME'"/g' $MANIFEST_FILE
#sed -i '' 's/versionName="'$VERSIONNAME'"/versionName="'$NEWVERSIONNAME'"/g' $MANIFEST_FILE
#fi

