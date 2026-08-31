# NVMe cache capacity expectations

30 GB NVMe is not:

```text
30 GB database limit
```

It is:

```text
30 GB hot block working set accelerator
```

## If active indexes + hot table pages fit substantially within 30 GB

```text
excellent
```

## If hot working set becomes 200 GB

```text
cache churn increases
HDD reads increase
```

Metrics should tell you when this happens.

Then upgrade decisions are evidence-based.
