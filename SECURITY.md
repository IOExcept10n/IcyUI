# Security policy

## Supported versions

IcyUI is in early alpha. Only the latest published prerelease receives security fixes. Older alphas aren't patched; upgrade to the latest one.

| Version | Supported |
|---|---|
| Latest `0.1.0-alpha.*` on NuGet | Yes |
| Older alphas | No |

## Reporting a vulnerability

Please don't open a public issue for a security problem.

Report it privately through GitHub's [private vulnerability reporting](https://github.com/IOExcept10n/IcyUI/security/advisories/new). Include:

- the affected package and version;
- the steps or code needed to reproduce the problem;
- the impact you expect, for example a crash, data exposure or code execution.

You'll get an acknowledgement within about 7 days. The maintainer will confirm whether the report is a vulnerability, work on a fix, and credit you in the advisory unless you ask not to be named. Please keep the details private until a fix is released.

## Scope

In scope: the `IcyUI`, `IcyUI.Design`, `IcyUI.MonoGame` and `IcyUI.Stride` packages, and the release and CI workflows in this repository.

Out of scope: problems in Stride, MonoGame or other third-party engines and libraries. Report those to their own projects. Crashes or visual bugs without a security impact belong in a normal bug report.
