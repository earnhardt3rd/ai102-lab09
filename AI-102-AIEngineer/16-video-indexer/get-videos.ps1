# Account ID was obtained from https://www.videoindexer.ai/account/2f8dc89e-57d4-4e8b-a1f4-ce67a8b7456c/settings
$account_id="2f8dc89e-57d4-4e8b-a1f4-ce67a8b7456c"
$api_key="2bf55784065f466ea3ba39aa0419a8f9"
$location="trial"

# Call the AccessToken method with the API key in the header to get an access token
$token = Invoke-RestMethod -Method "Get" -Uri "https://api.videoindexer.ai/auth/$location/Accounts/$account_id/AccessToken" -Headers @{'Ocp-Apim-Subscription-Key' = $api_key}

# Use the access token to make an authenticated call to the Videos method to get a list of videos in the account
Invoke-RestMethod -Method "Get" -Uri "https://api.videoindexer.ai/$location/Accounts/$account_id/Videos?accessToken=$token" | ConvertTo-Json
