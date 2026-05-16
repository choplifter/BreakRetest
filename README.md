# Documentation: Break-Retest Pro Trading Indicator (NinjaTrader 8)

## 1. Introduction & Core Strategy Concept

The **Break-Retest Strategy** is one of the pillars of structural technical analysis used extensively by both systematic retail algorithms and institutional desks. It relies on the principle of market memory: once a major structural support or resistance level is broken with decisive volume, the supply/demand equilibrium shifts, reversing the level's functional role. A former ceiling (resistance) becomes a new floor (support), and vice-versa.

Traditional technical indicators often fail to capture this pattern because they evaluate price lines strictly as zero-width points. This indicator mitigates failure points by adapting two essential paradigms:
1. **Dynamic Price Zones:** Financial instruments rarely pivot down to the exact tick. Institutional participants cluster orders inside price ranges, often creating liquidity sweeps (stop hunts) slightly beyond precise levels. This indicator encapsulates levels inside mathematically calculated buffers.
2. **Deterministic State-Machine Control:** Rather than relying on a loose matrix of unstructured variables or global boolean flags (`isBroken`, `hadRetest`), each identified zone runs an autonomous structural life cycle. This minimizes memory leaks, optimizes performance, and isolates trading signals cleanly from market noise.

---

## 2. Technical Schematic & Visualizing the Setup

Below is a technical layout illustrating how a resistance zone shifts through states from initial definition to execution.

![Break-Retest Lifecycle Schematic](data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCA3MDAgNDAwIiB3aWR0aD0iMTAwJSIgaGVpZ2h0PSJhdXRvIiBzdHlsZT0iYmFja2dyb3VuZC1jb2xvcjogIzFhMWUyNDsgZm9udC1mYW1pbHk6ICdTZWdvZSBVSScsIHNhbnMtc2VyaWY7IGJvcmRlci1yYWRpdXM6IDhweDsiPgogIDwhLS0gR3JpZCBMaW5lcyAtLT4KICA8ZGVmcz4KICAgIDxwYXR0ZXJuIGlkPSJncmlkIiB3aWR0aD0iNDAiIGhlaWdodD0iNDAiIHBhdHRlcm5Vbml0cz0idXNlclNwYWNlT25Vc2UiPgogICAgICA8cGF0aCBkPSJNIDQwIDAgTCAwIDAgMCA0MCIgZmlsbD0ibm9uZSIgc3Ryb2tlPSIjMmQzNTQwIiBzdHJva2Utd2lkdGg9IjEiLz4KICAgIDwvcGF0dGVybj4KICA8L2RlZnM+CiAgPHJlY3Qgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgZmlsbD0iIzE2MTkxZSIvPgogIDxyZWN0IHdpZHRoPSIxMDAlIiBoZWlnaHQ9IjEwMCUiIGZpbGw9InVybCgjZ3JpZCkiLz4KCiAgPCEtLSBSZXNpc3RhbmNlIExldmVsIEFyZWEgKEluaXRpYWwgU3RhdGUpIC0tPgogIDxyZWN0IHg9IjUwIiB5PSIxODAiIHdpZHRoPSIyODAiIGhlaWdodD0iMjAiIGZpbGw9IiNmODcxNzEiIGZpbGwtb3BhY2l0eT0iMC4xNSIgc3Ryb2tlPSIjZjg3MTcxIiBzdHJva2Utd2lkdGg9IjEiIHN0cm9rZS1kYXNoYXJyYXk9IjQiLz4KICA8dGV4dCB4PSI2MCIgeT0iMTk0IiBmaWxsPSIjZjg3MTcxIiBmb250LXNpemU9IjEyIiBmb250LXdlaWdodD0iYm9sZCI+QUNUSVZFIFJFU0lTVEFOQ0UgWk9ORSAoU3RhdGU6IEFjdGl2ZSk8L3RleHQ+CgogIDwhLS0gQnJva2VuIExldmVsIEFyZWEgLS0+CiAgPHJlY3QgeD0iMzMwIiB5PSIxODAiIHdpZHRoPSIxNDAiIGhlaWdodD0iMjAiIGZpbGw9IiMzOGJkZjgiIGZpbGwtb3BhY2l0eT0iMC4xNSIgc3Ryb2tlPSIjMzhiZGY4IiBzdHJva2Utd2lkdGg9IjEuNSIvPgogIDx0ZXh0IHg9IjMzNSIgeT0iMTk0IiBmaWxsPSIjMzhiZGY4IiBmb250LXNpemU9IjEyIiBmb250LXdlaWdodD0iYm9sZCI+QlJPS0VOIChTdGF0ZTogQnJva2VuKTwvdGV4dD4KCiAgPCEtLSBSZXRlc3RpbmcgQXJlYSAtLT4KICA8cmVjdCB4PSI0NzAiIHk9IjE4MCIgd2lkdGg9IjEwMCIgaGVpZ2h0PSIyMCIgZmlsbD0iI2VhYjMwOCIgZmlsbC1vcGFjaXR5PSIwLjIiIHN0cm9rZT0iI2VhYjMwOCIgc3Ryb2tlLXdpZHRoPSIyIi8+CiAgPHRleHQgeD0iNDc1IiB5PSIxOTQiIGZpbGw9IiNlYWIzMDgiIGZvbnQtc2l6ZT0iMTIiIGZvbnQtd2VpZ2h0PSJib2xkIj5SRVRFU1RJTkc8L3RleHQ+CgogIDwhLS0gQ29uZmlybWVkIEFyZWEgLS0+CiAgPHJlY3QgeD0iNTcwIiB5PSIxODAiIHdpZHRoPSIxMDAiIGhlaWdodD0iMjAiIGZpbGw9IiMyMmM1NWUiIGZpbGwtb3BhY2l0eT0iMC4xNSIgc3Ryb2tlPSIjMjJjNTVlIiBzdHJva2Utd2lkdGg9IjEiIHN0cm9rZS1kYXNoYXJyYXk9IjQiLz4KCiAgPCEtLSBQcmljZSBBY3Rpb24gUGF0aCAtLT4KICA8cGF0aCBkPSJNIDUwIDMyMCBMIDEyMCAyNjAgTCAxODAgMjkwIEwgMjYwIDIwMCBMIDMwMCAyMTUgTCAzNjAgMTEwIEwgNDIwIDEzMCBMIDQ5MCAxODUgTCA1MjAgMTkwIEwgNjAwIDgwIEwgNjYwIDEwMCIgCiAgICAgICAgZmlsbD0ibm9uZSIgc3Ryb2tlPSIjZTJlOGYwIiBzdHJva2Utd2lkdGg9IjMiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIgc3Ryb2tlLWxpbmVqb2luPSJyb3VuZCIvPgoKICA8IS0tIEtleSBBY3Rpb24gTWFya2VycyAtLT4KICA8IS0tIEJyZWFrb3V0IHBvaW50IC0tPgogIDxjaXJjbGUgY3g9IjM0MCIgY3k9IjE0MCIgcj0iNiIgZmlsbD0iIzM4YmRmOCIvPgogIDxsaW5lIHgxPSIzNDAiIHkxPSIxNDAiIHgyPSIzNDAiIHkyPSI4MCIgc3Ryb2tlPSIjMzhiZGY4IiBzdHJva2Utd2lkdGg9IjEiIHN0cm9rZS1kYXNoYXJyYXk9IjIiLz4KICA8dGV4dCB4PSIzNDUiIHk9Ijc1IiBmaWxsPSIjMzhiZGY4IiBmb250LXNpemU9IjExIj5JbXB1bHNpdmUgQnJlYWtvdXQ8YnIvPihDYW5kbGUgY2xvc2VzIGFib3ZlIHpvbmUpPC90ZXh0PgoKICA8IS0tIFJldGVzdCBwb2ludCAtLT4KICA8Y2lyY2xlIGN4PSI1MDUiIGN5PSIxODgiIHI9IjYiIGZpbGw9IiNlYWIzMDgiLz4KICA8dGV4dCB4PSI0NDAiIHk9IjIzMCIgZmlsbD0iI2VhYjMwOCIgZm9udC1zaXplPSIxMSI+VG91Y2ggb2YgWm9uZSBMaW1pdHM8L3RleHQ+CiAgPHBhdGggZD0iTSA0OTAgMjIwIFEgNTA1IDIwNSA1MDUgMTk1IiBmaWxsPSJub25lIiBzdHJva2U9IiNlYWIzMDgiIHN0cm9rZS13aWR0aD0iMSIgbWFya2VyLWVuZD0idXJsKCNhcnJvdykiLz4KCiAgPCEtLSBFbnRyeSBTaWduYWwgQXJyb3cgLS0+CiAgPHBhdGggZD0iTSA1MzAgMjQwIEwgNTMwIDIxMCIgZmlsbD0ibm9uZSIgc3Ryb2tlPSIjMjJjNTVlIiBzdHJva2Utd2lkdGg9IjMiIG1hcmtlci1lbmQ9InVybCgjZ3JlZW4tYXJyb3cpIi8+CiAgPGRlZnM+CiAgICA8bWFya2VyIGlkPSJncmVlbi1hcnJvdyIgdmlld0JveD0iMCAwIDEwIDEwIiByZWZYPSI1IiByZWZZPSIwIiBtYXJrZXJXaWR0aD0iNiIgbWFya2VySGVpZ2h0PSI2IiBvcmllbnQ9ImF1dG8tc3RhcnQtcmV2ZXJzZSI+CiAgICAgIDxwYXRoIGQ9Ik0gMCAxMCBMIDUgMCBMIDEwIDEwIFoiIGZpbGw9IiMyMmM1NWUiLz4KICAgIDwvbWFya2VyPgogIDwvZGVmcz4KICA8dGV4dCB4PSI1NDUiIHk9IjIyNSIgZmlsbD0iIzIyYzU1ZSIgZm9udC1zaXplPSIxMyIgZm9udC13ZWlnaHQ9ImJvbGQiPkVOVFJZIChTdGF0ZTogQ29uZmlybWVkKTwvdGV4dD4KICA8dGV4dCB4PSI1NDUiIHk9IjI0MCIgZmlsbD0iI2E3ZjNkMCIgZm9udC1zaXplPSIxMSI+RW5ndWxmaW5nICsgVk9MIFNwaWtlPC90ZXh0PgoKICA8IS0tIEF4aXMgTGFiZWxzIC0tPgogIDx0ZXh0IHg9IjYzMCIgeT0iMzgwIiBmaWxsPSIjNjQ3NDhiIiBmb250LXNpemU9IjEyIj5UaW1lIOKGkjwvdGV4dD4KICA8dGV4dCB4PSIyMCIgeT0iNDAiIGZpbGw9IiM2NDc0OGIiIGZvbnQtc2l6ZT0iMTIiPlByaWNlIOKGkTwvdGV4dD4KPC9zdmc+)

---

## 3. The State-Machine Lifecycle

Every single zone tracked in the repository operates within a strict algorithmic lifecycle. The programmatic flow transitions sequentially as follows:

![State Machine Architecture](data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCA4MDAgMjYwIiB3aWR0aD0iMTAwJSIgaGVpZ2h0PSJhdXRvIiBzdHlsZT0iYmFja2dyb3VuZC1jb2xvcjogIzFhMWUyNDsgZm9udC1mYW1pbHk6ICdTZWdvZSBVSScsIHNhbnMtc2VyaWY7IGJvcmRlci1yYWRpdXM6IDhweDsiPgogIDxyZWN0IHdpZHRoPSIxMDAlIiBoZWlnaHQ9IjEwMCUiIGZpbGw9IiMxNjE5MWUiLz4KICAKICA8ZGVmcz4KICAgIDxtYXJrZXIgaWQ9ImFycm93IiB2aWV3Qm94PSIwIDAgMTAgMTAiIHJlZlg9IjYiIHJlZlk9IjUiIG1hcmtlcldpZHRoPSI2IiBtYXJrZXJIZWlnaHQ9IjYiIG9yaWVudD0iYXV0by1zdGFydC1yZXZlcnNlIj4KICAgICAgPHBhdGggZD0iTSAwIDEgTCAxMCA1IEwgMCA5IHoiIGZpbGw9IiM5NGEzYjgiLz4KICAgIDwvbWFya2VyPgogICAgPG1hcmtlciBpZD0iYXJyb3ctcmVkIiB2aWV3Qm94PSIwIDAgMTAgMTAiIHJlZlg9IjYiIHJlZlk9IjUiIG1hcmtlcldpZHRoPSI2IiBtYXJrZXJIZWlnaHQ9IjYiIG9yaWVudD0iYXV0by1zdGFydC1yZXZlcnNlIj4KICAgICAgPHBhdGggZD0iTSAwIDEgTCAxMCA1IEwgMCA5IHoiIGZpbGw9IiNlZjQ0NDQiLz4KICAgIDwvbWFya2VyPgogIDwvZGVmcz4KCiAgPCEtLSBTdGF0ZSAwOiBBY3RpdmUgLS0+CiAgPHJlY3QgeD0iMzAiIHk9IjkwIiB3aWR0aD0iMTEwIiBoZWlnaHQ9IjYwIiByeD0iNiIgZmlsbD0iI2Y4NzE3MSIgZmlsbC1vcGFjaXR5PSIwLjEiIHN0cm9rZT0iI2Y4NzE3MSIgc3Ryb2tlLXdpZHRoPSIyIi8+CiAgPHRleHQgeD0iODUiIHk9IjEyMCIgZmlsbD0iI2Y4NzE3MSIgZm9udC13ZWlnaHQ9ImJvbGQiIGZvbnQtc2l6ZT0iMTQiIHRleHQtYW5jaG9yPSJtaWRkbGUiPkFDVElWRTwvdGV4dD4KICA8dGV4dCB4PSI4NSIgeT0iMTM4IiBmaWxsPSIjOTRhM2I4IiBmb250LXNpemU9IjEwIiB0ZXh0LWFuY2hvcj0ibWlkZGxlIj5MZXZlbCBJZGVudGlmaWVkPC90ZXh0PgoKICA8IS0tIFRyYW5zaXRpb24gMSAtLT4KICA8bGluZSB4MT0iMTQwIiB5MT0iMTIwIiB4Mj0iMjAwIiB5Mj0iMTIwIiBzdHJva2U9IiM5NGEzYjgiIHN0cm9rZS13aWR0aD0iMiIgbWFya2VyLWVuZD0idXJsKCNhcnJvdykiLz4KICA8dGV4dCB4PSIxNzAiIHk9IjExMCIgZmlsbD0iIzM4YmRmOCIgZm9udC1zaXplPSIxMCIgdGV4dC1hbmNob3I9Im1pZGRsZSI+QnJlYWtvdXQ8L3RleHQ+CgogIDwhLS0gU3RhdGUgMTogQnJva2VuIC0tPgogIDxyZWN0IHg9IjIxMCIgeT0iOTAiIHdpZHRoPSIxMTAiIGhlaWdodD0iNjAiIHJ4PSI2IiBmaWxsPSIjMzhiZGY4IiBmaWxsLW9wYWNpdHk9IjAuMSIgc3Ryb2tlPSIjMzhiZGY4IiBzdHJva2Utd2lkdGg9IjIiLz4KICA8dGV4dCB4PSIyNjUiIHk9IjEyMCIgZmlsbD0iIzM4YmRmOCIgZm9udC13ZWlnaHQ9ImJvbGQiIGZvbnQtc2l6ZT0iMTQiIHRleHQtYW5jaG9yPSJtaWRkbGUiPkJST0tFTjwvdGV4dD4KICA8dGV4dCB4PSIyNjUiIHk9IjEzOCIgZmlsbD0iIzk0YTNiOCIgZm9udC1zaXplPSIxMCIgdGV4dC1hbmNob3I9Im1pZGRsZSI+Q2FuZGxlIENsb3NlIE91dDwvdGV4dD4KCiAgPCEtLSBUcmFuc2l0aW9uIDIgLS0+CiAgPGxpbmUgeDE9IjMyMCIgeTE9IjEyMCIgeDI9IjM4MCIgeTI9IjEyMCIgc3Ryb2tlPSIjOTRhM2I4IiBzdHJva2Utd2lkdGg9IjIiIG1hcmtlci1lbmQ9InVybCgjYXJyb3cpIi8+CiAgPHRleHQgeD0iMzUwIiB5PSIxMTAiIGZpbGw9IiNlYWIzMDgiIGZvbnQtc2l6ZT0iMTAiIHRleHQtYW5jaG9yPSJtaWRkbGUiPlByaWNlIFJldHVybjwvdGV4dD4KCiAgPCEtLSBTdGF0ZSAyOiBSZXRlc3RpbmcgLS0+CiAgPHJlY3QgeD0iMzkwIiB5PSI5MCIgd2lkdGg9IjExMCIgaGVpZ2h0PSI2MCIgcng9IjYiIGZpbGw9IiNlYWIzMDgiIGZpbGwtb3BhY2l0eT0iMC4xIiBzdHJva2U9IiNlYWIzMDgiIHN0cm9rZS13aWR0aD0iMiIvPgogIDx0ZXh0IHg9IjQ0NSIgeT0iMTIwIiBmaWxsPSIjZWFiMzA4IiBmb250LXdlaWdodD0iYm9sZCIgZm9udC1zaXplPSIxNCIgdGV4dC1hbmNob3I9Im1pZGRsZSI+UkVURVNUSU5HPC90ZXh0PgogIDx0ZXh0IHg9IjQ0NSIgeT0iMTM4IiBmaWxsPSIjOTRhM2I4IiBmb250LXNpemU9IjEwIiB0ZXh0LWFuY2hvcj0ibWlkZGxlIj5JbnNpZGUgWm9uZTwvdGV4dD4KCiAgPCEtLSBUcmFuc2l0aW9uIDM6IFN1Y2Nlc3MgLS0+CiAgPHBhdGggZD0iTSA1MDAgMTEwIEwgNTkwIDc1IiBmaWxsPSJub25lIiBzdHJva2U9IiMyMmM1NWUiIHN0cm9rZS13aWR0aD0iMiIgbWFya2VyLWVuZD0idXJsKCNhcnJvdykiLz4KICA8dGV4dCB4PSI1NDUiIHk9IjgwIiBmaWxsPSIjMjJjNTVlIiBmb250LXNpemU9IjEwIiB0ZXh0LWFuY2hvcj0ibWlkZGxlIj5UcmlnZ2VyIFZhbGlkPC90ZXh0PgoKICA8IS0tIFN0YXRlIDNhOiBDb25maXJtZWQgLS0+CiAgPHJlY3QgeD0iNjAwIiB5PSIzMCIgd2lkdGg9IjE0MCIgaGVpZ2h0PSI2MCIgcng9IjYiIGZpbGw9IiMyMmM1NWUiIGZpbGwtb3BhY2l0eT0iMC4xIiBzdHJva2U9IiMyMmM1NWUiIHN0cm9rZS13aWR0aD0iMiIvPgogIDx0ZXh0IHg9IjY3MCIgeT0iNjAiIGZpbGw9IiMyMmM1NWUiIGZvbnQtd2VpZ2h0PSJib2xkIiBmb250LXNpemU9IjE0IiB0ZXh0LWFuY2hvcj0ibWlkZGxlIj5DT05GSVJNRUQ8L3RleHQ+CiAgPHRleHQgeD0iNjcwIiB5PSI3OCIgZmlsbD0iIzk0YTNiOCIgZm9udC1zaXplPSIxMCIgdGV4dC1hbmNob3I9Im1pZGRsZSI+U2lnbmFsIEZpcmVkIC8gRW50cnk8L3RleHQ+CgogIDwhLS0gVHJhbnNpdGlvbiA0OiBGYWlsdXJlIC0tPgogIDxwYXRoIGQ9Ik0gNTAwIDEzMCBMIDU5MCAxNjUiIGZpbGw9Im5vbmUiIHN0cm9rZT0iI2VmNDQ0NCIgc3Ryb2tlLXdpZHRoPSIyIiBtYXJrZXItZW5kPSJ1cmwoI2Fycm93LXJlZCkiLz4KICA8dGV4dCB4PSI1NDUiIHk9IjE2MCIgZmlsbD0iI2VmNDQ0NCIgZm9udC1zaXplPSIxMCIgdGV4dC1hbmNob3I9Im1pZGRsZSI+Wm9uZSBWaW9sYXRlZDwvdGV4dD4KCiAgPCEtLSBTdGF0ZSAzYjogSW52YWxpZGF0ZWQgLS0+CiAgPHJlY3QgeD0iNjAwIiB5PSIxNTAiIHdpZHRoPSIxNDAiIGhlaWdodD0iNjAiIHJ4PSI2IiBmaWxsPSIjZWY0NDQ0IiBmaWxsLW9wYWNpdHk9IjAuMSIgc3Ryb2tlPSIjZWY0NDQ0IiBzdHJva2Utd2lkdGg9IjIiLz4KICA8dGV4dCB4PSI2NzAiIHk9IjE4MCIgZmlsbD0iI2VmNDQ0NCIgZm9udC13ZWlnaHQ9ImJvbGQiIGZvbnQtc2l6ZT0iMTQiIHRleHQtYW5jaG9yPSJtaWRkbGUiPklOVkFMSURBVEVEPC90ZXh0PgogIDx0ZXh0IHg9IjY3MCIgeT0iMTk4IiBmaWxsPSIjOTRhM2I4IiBmb250LXNpemU9IjEwIiB0ZXh0LWFuY2hvcj0ibWlkZGxlIj5TZXR1cCBBYm9ydGVkPC90ZXh0PgoKICA8IS0tIERpcmVjdCBJbnZhbGlkYXRpb24gLS0+CiAgPHBhdGggZD0iTSAyNjUgMTUwIFEgMjY1IDIzMCA0NjAgMjMwIFEgNjUwIDIzMCA2NTAgMjEyIiBmaWxsPSJub25lIiBzdHJva2U9IiNlZjQ0NDQiIHN0cm9rZS13aWR0aD0iMS41IiBzdHJva2UtZGFzaGFycmF5PSIzIiBtYXJrZXItZW5kPSJ1cmwoI2Fycm93LXJlZCkiLz4KICA8dGV4dCB4PSI0NDUiIHk9IjIyNSIgZmlsbD0iI2VmNDQ0NCIgZm9udC1zaXplPSIxMCIgdGV4dC1hbmNob3I9Im1pZGRsZSI+Q2xvc2VzIGNvbXBsZXRlbHkgdGhyb3VnaCB6b25lIChObyB2YWxpZCByZXRlc3QpPC90ZXh0Pgo8L3N2Zz4=)

1. **`Active`:** The level has been validated by price action (e.g., a structural swing point). Price remains bounded on the native side of the level.
2. **`Broken`:** An asset bar achieves a confirmed close completely penetrating the outer boundaries of the zone. This confirms structural breakout momentum.
3. **`Retesting`:** Price mean-reverts back into the breakout zone's boundaries. The system marks this zone as a high-priority "Point of Interest" (POI) and watches for buyer/seller absorption.
4. **`Confirmed`:** A specified entry trigger is met inside or immediately exiting the zone parameters. A trade execution signal is dispatched, and drawing markers pin onto the user interface.
5. **`Invalidated`:** The counter-trend momentum violates the level (e.g., price collapses fully back below a broken resistance zone and closes on the wrong side). The state engine drops tracking to protect capital.

---

## 4. Structural Level Generation Engines

To capture institutional interest efficiently, the indicator combines macro session milestones with microscopic price action structures:

### A. High-Timeframe Context: Prior Day Levels (OHLC)
Commercial market makers execute major orders around prior day boundaries due to resting order book liquidity. When enabled, the indicator references the market's initial session bars to calculate:
* **Previous Day High (PDH):** Represents the ultimate supply ceiling of the previous day. Breaking it signals macro bullish expansion.
* **Previous Day Low (PDL):** Represents the ultimate demand floor of the previous day. Breaking it signals macro liquidation expansion.

*Note: HTF session zones are drawn with highly saturated, prominent fills (**Dark Red** and **Dark Green**) to immediately communicate macro importance.*

### B. Micro-Structure: Fractal Swing Highs & Lows (Pivots)
For intra-day execution, the engine runs a localized scanning routine using an **N-Bar Extremum Rule**:
* A **Swing High** is locked if a bar high is higher than the highs of *N* bars prior and *N* bars subsequent.
* A **Swing Low** is locked if a bar low is lower than the lows of *N* bars prior and *N* bars subsequent.

*The N variable is configured via the user property `Swing Strength`.*

---

## 5. Visual Dashboard & UI Color Legend

The geometric properties plotted by the indicator dynamically update opacity, boundaries, and colors based on real-time structural shifts:

### Tracking Zones (Rectangles)
* **🟢 Light Green (`LightGreen`):** Micro structural support zone tracked from a local fractal Swing Low.
* **🔴 Light Red (`LightCoral`):** Micro structural resistance zone tracked from a local fractal Swing High.
* **🟩 Dark Green (`SeaGreen`):** High-timeframe macro support zone derived from the **Previous Day Low (PDL)**.
* **🟥 Dark Red (`OrangeRed`):** High-timeframe macro resistance zone derived from the **Previous Day High (PDH)**.
* **🟦 Light Blue (`LightSkyBlue` with dark blue border):** Level has been successfully penetrated (`Broken` State). Currently waiting for price to correct back into the zone.
* **🟨 Yellow (`Yellow` with golden border):** **Critical Alert Mode**. Price is currently inside the zone limits (`Retesting` State). The algorithm is actively scanning for trigger conditions.

### UI Execution Signals
* **⬆️ Lime Green Arrow (`Lime`):** A buy signal confirmed by structural absorption and your selected trigger engine.
* **⬇️ Solid Red Arrow (`Red`):** A sell signal confirmed by structural absorption and your selected trigger engine.
* **📝 Text Marker "VOL":** Appends to the arrow if the entry was verified using the advanced Institutional Volume Filter.

---

## 6. Advanced Entry Engines & The Institutional Volume Filter

Once a tracking zone transitions to the `Retesting` state, execution is delegated to the user's chosen confirmation engine:

```
[Simple Price Action]  --> Executes immediately on a single color-conforming close.
[Engulfing Pattern]   --> Executes on a 2-bar structural momentum reversal.
[Volume-Backed Eng.]  --> Evaluates the 2-bar reversal against an institutional volume matrix.
```

The flagship engine **`VolumeBackedEngulfing`** filters retail noise by applying an algorithmic threshold. When an engulfing pattern completes, the system queries the previous 20 bars to compute an exact volume moving average ($V_{avg}$). The entry is authorized **only if** the breakout reversal bar ($V_{0}$) contains substantial institutional presence:

$$V_{0} > V_{avg} \times 1.2$$

This prevents taking entries during low-liquidity periods (e.g., market lunch hours, bank holidays) where price can drift aimlessly through key levels. A **20%+ spike** confirms that capitalization is actively defending the structural transition.

---

## 7. Parameters Configuration Guide

| UI Parameter Name | Data Type | Default | Operational Description |
| :--- | :--- | :--- | :--- |
| **`Swing Strength`** | Integer | `5` | Specifies the window size for local fractal pivots. A value of 5 requires 5 bars to the left and 5 bars to the right to be lower/higher. Increase for cleaner major swings. |
| **`Zone Tolerance (Ticks)`** | Integer | `4` | Controls the vertical radius of the zone from the exact peak/trough price tick. For high-volatility indices (e.g., Nasdaq Future / NQ), scale this up (e.g., 8–14 Ticks). |
| **`Use Prior Day Levels`** | Boolean | `true` | Toggles the automatic extraction, mapping, and state-tracking of Previous Day High/Low boundaries. |
| **`Trigger Logic`** | Enum | `VolumeBackedEngulfing` | Programmatic selection of the entry validation layer: `SimplePriceAction`, `Engulfing`, or `VolumeBackedEngulfing`. |

---

## 8. Installation & Compilation Instructions (NinjaTrader 8)

To seamlessly deploy the production script into your local environment:

1. Launch **NinjaTrader 8**.
2. Navigate to the top control center menu: **New** $\rightarrow$ **NinjaScript Editor**.
3. In the right-hand side panel (*NinjaScript Explorer*), right-click on the **Indicators** category folder and select **New Indicator...**
4. Set the name to exactly: **`BreakRetest`** *(Note: exact casing is mandatory to preserve class definition bindings)*. Click *Next*, then *Finish*.
5. Inside the code workspace, select all code text by pressing **`Ctrl + A`** and delete it entirely via **`Delete`**. The file workspace must be 100% blank.
6. Copy the compiled final C# source code provided in the conversation thread.
7. Paste it cleanly into the empty script file workspace (**`Ctrl + V`**).
8. Press **`F5`** on your keyboard (or click the green **Compile** button in the top menu layout).
9. Watch for the gray loading status bar at the bottom. Once you hear the audible confirmation chime, compilation has succeeded.
10. Open a live chart (e.g., E-mini S&P 500 / ES), right-click the workspace, select **Indicators**, choose **`BreakRetest`**, and click **OK**.

---

## 9. Risk Mitigation & Strategy Guidelines

### A. Precision Protective Stop-Loss (SL) Placement
* **Long Positions:** Place the protective SL 1 to 2 ticks below the absolute lowest wick achieved exclusively during the `Retesting` state sequence. This honors structural validation. If price violates this low, the pattern is structurally broken.
* **Short Positions:** Place the protective SL 1 to 2 ticks above the absolute highest wick achieved exclusively during the `Retesting` state sequence.

### B. Mathematical Risk-to-Reward Ratios (RRR)
Because the state engine tracks price reversion into the zone, entries are triggered close to the structural invalidation point. Ensure you target a minimum **RRR of 1:2**. The distance to your target (the original breakout swing extreme) should be at least double your structural stop distance.

### C. Market Regime Filtering
While the system operates bidirectionally, strategy win rates improve dramatically when trading inline with macro trends. If a higher timeframe (e.g., 1-Hour or 4-Hour chart) shows an established uptrend, ignore short entry alerts entirely and execute **only** on green long breakout-retest triggers.
