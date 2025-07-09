TEST ?= about

prep:
	docker compose up -d

run-api:
	dotnet run --project host

down:
	docker compose --profile all down
	docker volume prune -f

reset:
	make down
	make prep

assemble:
	docker compose --profile all up -d

build:
	docker compose --profile all build

yac:
	httpyac send tests/$(TEST).http --all --env local

replay:
	make reset
	make play

play:
	bash -c -i 'make run-api & sleep 5 && make yac && fg'

run-ui:
	npm run dev --prefix ui

consumer-play:
	make -C consumer/play play

