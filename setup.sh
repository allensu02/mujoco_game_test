#!/bin/bash

################################################
# Edit manifest.json
################################################
echo "Script running"
MANIFEST_FILE=$(realpath ./Packages/manifest.json)
NUGET_FOR_UNITY_URL="https://github.com/GlitchEnzo/NuGetForUnity.git?path=/src/NuGetForUnity"
BRAND_UNITY_URL="" # TODO: add link

add_project_dependency() {
    dependency="\"$1\": \"$2\","
    if (! (grep -q "\"$1\"" $MANIFEST_FILE) || (grep -q '^\/\/' $MANIFEST_FILE | grep -q "\"$1\"")); then
        sed -ie '/"dependencies": {/a\\t\t'"$dependency"'' $MANIFEST_FILE
    fi
}

add_project_dependency com.github-glitchenzo.nugetforunity $NUGET_FOR_UNITY_URL
add_project_dependency com.unity.nuget.newtonsoft-json 3.2.1

################################################
# Edit packages.config
################################################

CONFIG_FILE="./Assets/packages.config"

test -f $CONFIG_FILE || touch $CONFIG_FILE

# add root elements if needed
if (grep -Eq '<packages />|<packages/>' $CONFIG_FILE); then
   sed -ie '/<packages \/>\|<packages\/>/d' $CONFIG_FILE
   echo "<packages>" >> $CONFIG_FILE
   echo "</packages>" >> $CONFIG_FILE
fi

add_nuget_package() {
    if ($3); then
        manuallyInstalled="manuallyInstalled=\"true\" "
    else
        manuallyInstalled=""
    fi

    package="<package id=\"$1\" version=\"$2\" $manuallyInstalled/>"
    # echo $package

    if (! (grep -q "\"$1\"" $CONFIG_FILE) || (grep -Pzoq '<\!\-\-(.|\n)*\-\->' $CONFIG_FILE | grep -Pzoq "\"$1\"")); then
        sed -ie '/<packages>/a\\t'"$package"'' $CONFIG_FILE
    fi
}

# add StackExchange.Redis and its dependencies
add_nuget_package System.Runtime.CompilerServices.Unsafe 6.0.0 true
add_nuget_package Pipelines.Sockets.Unofficial 2.2.8 false
add_nuget_package Microsoft.Bcl.AsyncInterfaces 5.5.0 false
add_nuget_package Microsoft.Extensions.Logging.Abstractions 6.0.0 false
add_nuget_package System.IO.Pipelines 5.0.1 false
add_nuget_package System.Threading.Channels 5.0.0 false
add_nuget_package StackExchange.Redis 2.8.0 true # only the main package is marked manually/explicitly installed

################################################
# Install BRAND libraries
################################################

# TODO: replace with correct repo link
# curl -s https://api.github.com/repos/GlitchEnzo/NuGetForUnity/releases/latest \
# | grep "browser_download_url.*dll" \
# | cut -d : -f 2,3 \
# | tr -d \" \
# | wget -qi - -P ./Assets/Plugins/BRAND

echo "script done"