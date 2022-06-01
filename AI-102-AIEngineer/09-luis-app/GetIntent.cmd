@echo off

rem Set values for your Language Understanding app
set app_id=c5c5b497-3337-4190-93b8-833e3b15c3e9
set endpoint=https://ai102-luwfe.cognitiveservices.azure.com/
set key=938439491cba418ba0f2f441fbda4294

rem Get parameter and encode spaces for URL
set input=%1
set query=%input: =+%

rem Use cURL to call the REST API
curl -X GET "%endpoint%/luis/prediction/v3.0/apps/%app_id%/slots/production/predict?subscription-key=%key%&log=true&query=%query%"