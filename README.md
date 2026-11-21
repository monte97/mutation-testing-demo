# mutation-testing-demo

A small .NET 10 demo for a talk about what a high mutation score still misses.
It is a simplified rebuild of a real problem, not the real system: a wrong
aggregation that no test could see, because every test used a single value.
In the demo that bug is a `Max` where a `Sum` should be.

Speaker: Francesco Montelli ([montelli.dev](https://montelli.dev))

## What is here

- `demo/before/`: an order-processing service with a weak suite. 10 tests, all green.
- `demo/after/`: the same service with 22 tests, after two fixes guided by the mutation report.
- `demo/agent/`: the same service with an `AGENTS.md` of domain rules and a new one (orders above 1,000 € go to review). Used in the last part of the talk.
- `demo/agent-run/`: what the AI agents answered and what Stryker measured on `demo/agent`, recorded on 2026-11-21 (in Italian).
- `demo/saved-reports/`: pre-generated reports, readable without running anything.
- `slides.pdf`: slides from an earlier, Italian version of the talk (Working Software 2026).

## Numbers

Measured on .NET 10.0.301 with Stryker.NET 5.0.0 (Standard level, mutating `**/*Service.cs`):

| | Tests | Line coverage | Branch coverage | Mutation score | Killed | Survived | No coverage |
|---|---|---|---|---|---|---|---|
| `before` | 10 | 92.9% (66/71) | 80% (16/20) | 65.0% | 26 | 12 | 2 |
| `after` | 22 | not measured | not measured | 92.5% | 37 | 3 | 0 |
| `agent` | 25 | not measured | not measured | 97.7% | 43 | 1 | 0 |

The score counts killed mutants over killed, survived and not covered ones
(40 valid mutants in both cases). Compile errors and ignored mutants are left out.
All numbers come from this demo.

## The agent part

`demo/agent` asks a different question: can you trust an agent that says the tests cover the rules?
An agent wrote tests for the 1,000 € rule and covered the boundary. A second agent, reading the code
without running anything, said that rule 5 (a non-positive total goes to review) had no test, and named
the mutants that would survive. Stryker confirmed both and found a third one the agent had not looked at,
an equivalent mutant on the discount cap: 93.18% with the agent's tests. One more test for rule 5
brings it to 97.73%; only the equivalent mutant is left. The agent's answer was right, and it was still
a prediction until Stryker ran.

## Run it

You need Docker. From `demo/before/`, `demo/after/` or `demo/agent/`:

```bash
make build      # build the image (.NET 10 SDK, Stryker.NET 5.0.0)
make test       # run the tests
make stryker    # mutation testing, report in reports/
make coverage   # line and branch coverage, HTML report in reports/coverage/html
```

Stryker.NET and ReportGenerator are pinned in a local tool manifest
(`.config/dotnet-tools.json`), so nothing is installed globally. Without Docker,
with the .NET 10 SDK installed:

```bash
cd demo/before
dotnet tool restore
cd tests/MutationTestingDemo.Tests
dotnet stryker -f ../../stryker-config.json
```

## The three holes in the `before` suite

- `OrderService.cs:26`: `Sum()` to `Max()` survives, because each test has one item. `* Quantity` to `/ Quantity` survives too, because the quantity is always 1.
- `DiscountService.cs`: the boundary `>= 3 years` to `> 3 years` survives, because the test customer has 5 years.
- `OrderServiceTests.cs`: the order status is never asserted.

## License

The code is licensed under [Apache-2.0](LICENSE). The talk content (`slides.pdf`
and this README) is licensed under [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/).
