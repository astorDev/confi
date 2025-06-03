prep:
	docker compose up -d

run:
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