import json
from pathlib import Path

path = Path(r"C:\Users\uzayr\source\repos\billing-platform\postman\billing-platform.postman_collection.json")
obj = json.loads(path.read_text(encoding="utf-8"))
travel = next((x for x in obj["item"] if x.get("name") == "Travel Service"), None)
if travel is None:
    raise SystemExit("Travel Service section not found")

items = travel.get("item", [])
travel["description"] = "Travel CRM endpoints exposed through the gateway and direct public quote APIs. Confirmed itineraries should be created in booking context; quote-side itinerary paths are legacy compatibility flows."

if not any(i.get("name") == "Create Booking Itinerary" for i in items):
    booking_idx = next((idx for idx, i in enumerate(items) if i.get("name") == "Create Booking From Quotation"), len(items))
    items.insert(booking_idx + 1, {
        "name": "Create Booking Itinerary",
        "request": {
            "method": "POST",
            "header": [
                {"key": "Content-Type", "value": "application/json"},
                {"key": "x-tenant-id", "value": "{{tenantId}}"}
            ],
            "body": {
                "mode": "raw",
                "raw": "{\n  \"title\": \"Italy Confirmed Plan\",\n  \"destination\": \"Italy\",\n  \"startDate\": \"2026-06-10T09:00:00Z\",\n  \"endDate\": \"2026-06-20T18:00:00Z\",\n  \"travellers\": 2,\n  \"currency\": \"USD\",\n  \"items\": [\n    {\n      \"dayNumber\": 1,\n      \"itemType\": \"Other\",\n      \"title\": \"Arrival and transfer\",\n      \"description\": \"Airport pickup and hotel check-in\",\n      \"location\": \"Rome\",\n      \"startTime\": null,\n      \"endTime\": null,\n      \"cost\": 0,\n      \"currency\": \"USD\"\n    }\n  ]\n}"
            },
            "url": "{{travelBaseUrl}}/travel/bookings/{{bookingId}}/itinerary"
        },
        "event": [
            {
                "listen": "test",
                "script": {
                    "type": "text/javascript",
                    "exec": [
                        "const json = pm.response.json();",
                        "if (json && json.itineraryId) pm.environment.set('itineraryId', json.itineraryId);"
                    ]
                }
            }
        ]
    })

path.write_text(json.dumps(obj, indent=2), encoding="utf-8")
print("updated", path)
