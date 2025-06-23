import requests
from msal import PublicClientApplication

CLIENT_ID = '9f0a14a2-d7e2-4320-a961-8e8623f1511e'
AUTHORITY = 'https://login.microsoftonline.com/common'
SCOPES = ['Mail.Read']

app = PublicClientApplication(CLIENT_ID, authority=AUTHORITY)

flow = app.initiate_device_flow(scopes=SCOPES)
if 'user_code' not in flow:
    raise Exception("Failed to create device flow")

print(f"Go to {flow['verification_uri']} and enter code: {flow['user_code']}")

token = app.acquire_token_by_device_flow(flow)

if 'access_token' not in token:
    raise Exception("Failed to acquire token")

access_token = token['access_token']
headers = {'Authorization': f'Bearer {access_token}'}

response = requests.get('https://graph.microsoft.com/v1.0/me/mailFolders/inbox/messages?$top=10', headers=headers)
emails = response.json()

print("\n=== Your Inbox Emails ===")
for msg in emails.get('value', []):
    sender = msg['from']['emailAddress']['name']
    subject = msg['subject']
    preview = msg['bodyPreview']
    print(f"From: {sender}\nSubject: {subject}\nPreview: {preview}\n---")
