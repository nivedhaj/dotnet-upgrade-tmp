.PHONY: db build host test test-% list-tests

db:
	./scripts/setup-db.sh

build:
	./scripts/build-all.sh

host:
	./scripts/run-host.sh

test:
	./scripts/run-tests.sh all

list-tests:
	./scripts/run-tests.sh help

test-%:
	./scripts/run-tests.sh $*
