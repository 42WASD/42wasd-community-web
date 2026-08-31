# Load-test scenarios

At minimum:

```text
Scenario A:
10,000 connected/open clients
only 300 viewing forum
verify only those scopes receive forum realtime work

Scenario B:
1,000 users open same popular topic
measure RequestCoordinator/client + HybridCache/Dragonfly collapse

Scenario C:
500 users create comments over burst window
measure DB/outbox/SignalR

Scenario D:
large external API slowdown
verify page core remains responsive

Scenario E:
RabbitMQ outage
verify outbox safely accumulates

Scenario F:
Dragonfly flushed
verify controlled origin load and recovery

Scenario G:
SignalR disconnect/reconnect
verify delta repair

Scenario H:
HDD cold cache miss
measure latency against NVMe hit
```
