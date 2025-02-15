## [1.6.1](https://github.com/Faustvii/qbitHelper/compare/v1.6.0...v1.6.1) (2025-02-15)


### Bug Fixes

* changed connectable test to use external ip and not internal ([54327e5](https://github.com/Faustvii/qbitHelper/commit/54327e5fa013f297978536f57bb6cc96b621c291))

# [1.6.0](https://github.com/Faustvii/qbitHelper/compare/v1.5.4...v1.6.0) (2025-02-14)


### Features

* added new ensure qbittorrent connectable job ([0c3f9e5](https://github.com/Faustvii/qbitHelper/commit/0c3f9e50da3a16eb17b5c506cce7c0c4a018ab35))

## [1.5.4](https://github.com/Faustvii/qbitHelper/compare/v1.5.3...v1.5.4) (2025-02-11)


### Bug Fixes

* only mark as issue when no valid trackers ([a815cfc](https://github.com/Faustvii/qbitHelper/commit/a815cfc56b267d13bef55fe8eef1b4db38e1dc00))

## [1.5.3](https://github.com/Faustvii/qbitHelper/compare/v1.5.2...v1.5.3) (2025-02-11)


### Bug Fixes

* support qbittorrent 5+ ([898463f](https://github.com/Faustvii/qbitHelper/commit/898463f2550d9955428a7a4ef478bef57bb3dcce))

## [1.5.2](https://github.com/Faustvii/qbitHelper/compare/v1.5.1...v1.5.2) (2024-05-21)


### Bug Fixes

* fixed tag issue torrents always removing issue tag if it already had it ([006e65a](https://github.com/Faustvii/qbitHelper/commit/006e65a2bd21fb9db3cbad8d7f3786d7b73a99b5))

## [1.5.1](https://github.com/Faustvii/qbitHelper/compare/v1.5.0...v1.5.1) (2024-05-08)


### Bug Fixes

* fixed tag issue torrents tagging issue on torrents with the issue tag already ([8822873](https://github.com/Faustvii/qbitHelper/commit/882287314c83b434196bce6571dd4b807c4bc7e5))

# [1.5.0](https://github.com/Faustvii/qbitHelper/compare/v1.4.0...v1.5.0) (2024-05-08)


### Features

* added job to ensure qbittorrent preferences stay in sync ([20d31a4](https://github.com/Faustvii/qbitHelper/commit/20d31a4e3765bf1854edcb992dae12697536b22c))

# [1.4.0](https://github.com/Faustvii/qbitHelper/compare/v1.3.1...v1.4.0) (2024-04-16)


### Bug Fixes

* dryrun wasn't being listened to in reannouce racing job ([6d3b30e](https://github.com/Faustvii/qbitHelper/commit/6d3b30e9a5cf8d5f422182ee5a51b91d0275220c))


### Features

* added tag issue torrent job ([5e5b9e9](https://github.com/Faustvii/qbitHelper/commit/5e5b9e934cc830191f5480720270d748a4857df8))

## [1.3.1](https://github.com/Faustvii/qbitHelper/compare/v1.3.0...v1.3.1) (2024-04-16)


### Bug Fixes

* dependency injection error in reannounce job ([fb3d762](https://github.com/Faustvii/qbitHelper/commit/fb3d762800561b7832d5638cae035f15f0c1977c))

# [1.3.0](https://github.com/Faustvii/qbitHelper/compare/v1.2.5...v1.3.0) (2024-04-16)


### Features

* added option to exclude root folder of active torrent ([c443f87](https://github.com/Faustvii/qbitHelper/commit/c443f87d020adf8227eba37905b01f8f3e822655))
* added reannounce racing torrent job ([d79bcdf](https://github.com/Faustvii/qbitHelper/commit/d79bcdf25c3ce3c4d452ff095a660a5bdf4f5044))

## [1.2.5](https://github.com/Faustvii/qbitHelper/compare/v1.2.4...v1.2.5) (2024-04-15)


### Bug Fixes

* changed tag torrent job to be able to run on a shorter interval ([a64ecd2](https://github.com/Faustvii/qbitHelper/commit/a64ecd28c4ce2247683f6cf1de2f147f2d48b4cd))

## [1.2.4](https://github.com/Faustvii/qbitHelper/compare/v1.2.3...v1.2.4) (2024-04-14)


### Bug Fixes

* only delete folder if it has no files in it's subfolders ([77d585e](https://github.com/Faustvii/qbitHelper/commit/77d585e73e95da7e360cacafc1466186b628ef11))

## [1.2.3](https://github.com/Faustvii/qbitHelper/compare/v1.2.2...v1.2.3) (2024-04-14)


### Bug Fixes

* delete sub directories before parent directories ([560db5a](https://github.com/Faustvii/qbitHelper/commit/560db5a3d5b0442aa2c446d450b92e850b541894))

## [1.2.2](https://github.com/Faustvii/qbitHelper/compare/v1.2.1...v1.2.2) (2024-04-14)


### Bug Fixes

* single file torrents orphan were handled incorrectly ([5e0f1ac](https://github.com/Faustvii/qbitHelper/commit/5e0f1ac74290a6086e64a86232f16b382d6e63e5))

## [1.2.1](https://github.com/Faustvii/qbitHelper/compare/v1.2.0...v1.2.1) (2024-04-14)


### Bug Fixes

* obviously we should only use the temp path for in progress torrents ([9d7e657](https://github.com/Faustvii/qbitHelper/commit/9d7e6571f265f05a5846f5c3b249da8faf2f6074))

# [1.2.0](https://github.com/Faustvii/qbitHelper/compare/v1.1.6...v1.2.0) (2024-04-14)


### Bug Fixes

* when a torrent was not completely done, but some files where they would get wrongly orphaned ([0f59eb2](https://github.com/Faustvii/qbitHelper/commit/0f59eb294c158834804964ec2f94062c099a6208))


### Features

* added job to limit public torrent speeds ([0bfb034](https://github.com/Faustvii/qbitHelper/commit/0bfb0347e6754470b9a49790c5948b28e08a24e0))

## [1.1.6](https://github.com/Faustvii/qbitHelper/compare/v1.1.5...v1.1.6) (2024-04-14)


### Bug Fixes

* orphan job can sometimes fail with a nullpointer ([095f9cf](https://github.com/Faustvii/qbitHelper/commit/095f9cfab4e1c69869cfeed69ded5184a8d0cbc4))

## [1.1.5](https://github.com/Faustvii/qbitHelper/compare/v1.1.4...v1.1.5) (2024-04-14)


### Bug Fixes

* when torrents are stuck in metadata they won't pass the stall check ([51466aa](https://github.com/Faustvii/qbitHelper/commit/51466aa6a2519ef90e034ec18d4fd6272037ab02))

## [1.1.4](https://github.com/Faustvii/qbitHelper/compare/v1.1.3...v1.1.4) (2024-04-13)


### Bug Fixes

* Stalled torrents were considered before they were old ([ab02f70](https://github.com/Faustvii/qbitHelper/commit/ab02f706846132e30432c4bba467bbb4440a97e5))

## [1.1.3](https://github.com/Faustvii/qbitHelper/compare/v1.1.2...v1.1.3) (2024-04-13)


### Bug Fixes

* create destination directory before move ([6ad789f](https://github.com/Faustvii/qbitHelper/commit/6ad789f2e23c37538dd903125262104a4b529cef))

## [1.1.2](https://github.com/Faustvii/qbitHelper/compare/v1.1.1...v1.1.2) (2024-04-13)


### Bug Fixes

* check for file existing before trying to move it ([1dc8832](https://github.com/Faustvii/qbitHelper/commit/1dc8832a3c6a7a3b83dfb6ae6904f098f27e7aa5))

## [1.1.1](https://github.com/Faustvii/qbitHelper/compare/v1.1.0...v1.1.1) (2024-04-13)


### Bug Fixes

* fixed torrent privacy tag job doing the same work over and over ([f59b8f1](https://github.com/Faustvii/qbitHelper/commit/f59b8f1673bd7e6b4a1ddd8fab7a4979b13de4d9))

# [1.1.0](https://github.com/Faustvii/qbitHelper/compare/v1.0.0...v1.1.0) (2024-04-13)


### Features

* added exclude glob pattern support for orphan job ([cea16f0](https://github.com/Faustvii/qbitHelper/commit/cea16f07c90fd4774d33f649b3c493ffdd761451))
* load appsetting from /app/config/ to allow docker/kubernetes volume mount ([d30917f](https://github.com/Faustvii/qbitHelper/commit/d30917f2a71998cd3b1f11bf34e851ce6e47072a))

# 1.0.0 (2024-04-13)


### Features

* initial release ([e333c78](https://github.com/Faustvii/qbitHelper/commit/e333c781b5acbd9657dadb8656f6ba107ebc8ee2))
