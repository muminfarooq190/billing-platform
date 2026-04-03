"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const webhook_signer_service_1 = require("../src/signing/webhook-signer.service");
describe('WebhookSignerService', () => {
    it('signs payload deterministically', () => {
        const signer = new webhook_signer_service_1.WebhookSignerService();
        const signature = signer.sign('secret', '1700000000', '{"ok":true}');
        expect(signature.startsWith('sha256=')).toBe(true);
    });
});
//# sourceMappingURL=webhook-signer.service.spec.js.map