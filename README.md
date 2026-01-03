# Gargantua

A vendor-agnostic translation layer between industrial PLCs and an OpenAPI-based REST API.

## What Gargantua is

Gargantua is a headless edge middleware that:

- Exposes PLC data over a stable REST / OpenAPI contract
- Hides vendor-specific protocols (S7, ADS, EtherNet/IP, …) behind a single `IPlcProvider` interface
- Centralizes connection handling, timeouts, reconnects, logging and (later) caching and rate limiting
- Provides a simulation backend so clients can be developed and tested without real hardware

Typical consumers:

- SCADA / MES / HMIs
- Analytics / data collectors
- Tools, scripts, test harnesses
- Future RAG / “self-aware” services

## What Gargantua is not

- Not a SCADA or HMI
- Not an MES
- Not a tag browser
- Not responsible for machine logic or sequencing

Gargantua only provides **data access**, not process control.

## High-level architecture

- `Gargantua.Core`  
  Core models (`PlcAddress`, `PlcDataType`, `PlcValueQuality`, `PlcReadResult`, …)

- `Gargantua.Providers.Abstractions`  
  `IPlcProvider` and related abstractions for vendor implementations

- `Gargantua.Providers.Simulation`  
  In-memory simulated PLC implementing `IPlcProvider` (`SimPlc01` etc.)

- `Gargantua.Infrastructure`  
  Cross-cutting concerns (logging, configuration, later caching / rate limiting / health)

- `Gargantua.Api`  
  ASP.NET Core Web API that exposes the providers via HTTP and OpenAPI

## Current API surface (v0)

Simulation-only, single PLC:

- `POST /plcs/{plcIdentifier}/read`  
  Request:
  ```json
  { "addresses": [ "DB10.DBX0.0", "DB10.DBD4" ] }
