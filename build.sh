#!/bin/bash
set -o errexit

BTPATH=/home/blue/Games/BATTLETECH/game

RED='\033[0;31m'
NC='\033[0m' # No Color

if ! command -v nodemon &> /dev/null
then
    echo -e "${RED}nodemon could not be found, compiling once and exiting.${NC}"
    ~/.dotnet/dotnet build --verbosity normal --configuration Release -p:BattleTechGameDir='$BTPATH'
    exit
fi

nodemon -x "~/.dotnet/dotnet build --verbosity normal --configuration Release -p:BattleTechGameDir='$BTPATH' && rm -f 'EnvironmentalDesignMasks.zip' && cd '$BTPATH/Mods/EnvironmentalDesignMasks' && zip -x EnvironmentalDesignMasks.log -rq '$PWD/EnvironmentalDesignMasks.zip' ." -w src/ -e .
