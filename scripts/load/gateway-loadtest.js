import http from 'k6/http';
import { check } from 'k6';

export const options = {
  vus: 500,
  duration: '1m',
  thresholds: {
    http_req_duration: ['p(95)<250'],
    http_req_failed: ['rate<0.01'],
  },
};

export default function () {
  const res = http.get('http://localhost:5000/health');
  check(res, { 'status is 200': (r) => r.status === 200 });
}
