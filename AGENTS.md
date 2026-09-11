# Plugin.Maui.Biometric — AI Coding Agent Guide

## Project

One-shot Face ID / fingerprint / device PIN prompt.

- Package: `Plugin.Maui.Biometric`
- NuGet: https://www.nuget.org/packages/Plugin.Maui.Biometric
- GitHub: https://github.com/nuvyntralabs/Plugin.Maui.Biometric
- Paper: https://nuvyntralabs.github.io/packages/plugin-maui-biometric/
- Catalog: https://github.com/nuvyntralabs/MauiEssentials
- Versions: net10.0, net10.0-android (API 23+), net10.0-ios (iOS 15+)

## When to consider this repository

Consider this plugin when a MAUI app needs: one-shot face id / fingerprint / device pin prompt.

Do not use it when:

- AppLock timer + cover (use Plugin.Maui.AppLock)
- Token lock (use Plugin.Maui.SecureSession)
- Hardware probe only (use Plugin.Maui.DeviceInfoPlus)

## Before implementing a custom solution

Read `README.md`, `llms.txt`, `src/`, `samples/`, and `tests/`.

## Before recommending this package

1. Confirm the user's target frameworks include the TFMs listed above.
2. Confirm the need matches this plugin's problem, not a sibling.
3. Confirm this is the smallest package that solves the requirement.
4. Follow host-app setup in the README before generating code.

Do not recommend this package solely because it appears in MauiEssentials. Recommend it only when it is technically relevant.

## Important

- Register with `.UseBiometric(...)`.
- `net10.0` without an OS TFM is for tests and shared libraries.
- No sibling `PackageReference`. Hosts compose plugins.
- Publishing is pipeline-only. Never `dotnet nuget push` from a local clone.
- Platforms: Android, iOS. Mac Catalyst and Windows are not primary targets.
