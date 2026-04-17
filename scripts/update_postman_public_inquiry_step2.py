import json
from pathlib import Path

path = Path(r"C:\Users\uzayr\source\repos\billing-platform\postman\billing-platform.postman_collection.json")
obj = json.loads(path.read_text(encoding="utf-8"))
travel = next((x for x in obj["item"] if x.get("name") == "Travel Service"), None)
if travel is None:
    raise SystemExit("Travel Service section not found")

items = travel.get("item", [])
travel["description"] = "Travel CRM endpoints exposed through the gateway and direct public quote APIs. Includes public inquiry intake plus internal inquiry workflow. Confirmed itineraries should be created in booking context; quote-side itinerary paths are legacy compatibility flows."

if not any(i.get("name") == "Create Public Inquiry" for i in items):
    items.insert(0, {
        "name": "Create Public Inquiry",
        "request": {
            "auth": {"type": "noauth"},
            "method": "POST",
            "header": [
                {"key": "Content-Type", "value": "application/json"},
                {"key": "x-public-tenant-id", "value": "{{tenantId}}"}
            ],
            "body": {
                "mode": "raw",
                "raw": "{\n  \"fullName\": \"Jane Doe\",\n  \"email\": \"traveler@example.com\",\n  \"phone\": \"+15555550123\",\n  \"whatsappNumber\": \"+15555550123\",\n  \"departureCity\": \"Mumbai\",\n  \"destination\": \"Bali\",\n  \"travelDate\": \"2026-06-10T09:00:00Z\",\n  \"returnDate\": \"2026-06-16T18:00:00Z\",\n  \"isDateFlexible\": true,\n  \"travellers\": 2,\n  \"budgetAmount\": 150000,\n  \"budgetCurrency\": \"INR\",\n  \"message\": \"We want a honeymoon package with private transfers.\",\n  \"source\": \"Website\",\n  \"honeypot\": null\n}"
            },
            "url": "{{travelBaseUrl}}/travel/public/inquiries"
        },
        "event": [
            {
                "listen": "test",
                "script": {
                    "type": "text/javascript",
                    "exec": [
                        "const json = pm.response.json();",
                        "if (json && json.inquiryId) pm.environment.set('inquiryId', json.inquiryId);"
                    ]
                }
            }
        ]
    })

path.write_text(json.dumps(obj, indent=2), encoding="utf-8")
print("updated", path)
