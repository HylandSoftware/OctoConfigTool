#!/bin/bash
set -euo pipefail

SCRIPT_ROOT="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
CAKE_TASK=ci-test
IMAGE_TAG=0.0.1-ci
CAKE_ARGS=""

if [ "${TRAVIS_PULL_REQUEST}" = "false" ] && [ "${TRAVIS_BRANCH}" = "master" ]; then
    CAKE_TASK=ci-publish
	CAKE_ARGS="--nugetApiKey=${NUGET_API_KEY} --nugetPublishUrl=${NUGET_PUBLISH_URL}"
    #docker login -u "${DOCKER_USERNAME}" -p "${DOCKER_PASSWORD}"

    IMAGE_TAG=$(docker run -it --rm -v "$(pwd):/repo" gittools/gitversion-dotnetcore:linux-4.0.1 /repo /showvariable NuGetVersionV2)
    echo $IMAGE_TAG
    # Strip Trailing newlines that gitversion generated for us
    IMAGE_TAG=${IMAGE_TAG%$'\r'}
fi

echo "Building task ${CAKE_TASK}"

"${SCRIPT_ROOT}/../build.sh" -t "${CAKE_TASK}" "${CAKE_ARGS}" --version="${IMAGE_TAG}"
