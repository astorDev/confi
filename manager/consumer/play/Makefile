TEST ?= main

run:
	dotnet run

prep:
	make -C ../../ assemble

prep-build:
	make -C ../../ build

reprep:
	make prep-build
	make prep

kill:
	kill `lsof -t -i:53872`

yac:
	httpyac send $(TEST).http --all

play:
	bash -c -i 'make run & sleep 5 && make yac && kill -INT %1'

v-use:
	cp Versions/$(V)/Program.cs Program.cs

v-save:
	mkdir -p Versions/$(V) && cp Program.cs Versions/$(V)/Program.cs

v-play:
	make v-use
	make play

test:
	dotnet test --filter FullyQualifiedName~Confi.Manager.Consumer.Playground.$(NAME) --logger "console;verbosity=detailed"

log-debug:
	curl -X PUT http://localhost:40398/apps/confi-play/versions/unversioned/configuration \
		-H "Content-Type: application/json" \
		-d '{ "Logging": { "LogLevel": { "Confi.JsonHttpConfiguration.Source": "Debug" }}}'

log-info:
	curl -X PUT http://localhost:40398/apps/confi-play/versions/unversioned/configuration \
		-H "Content-Type: application/json" \
		-d '{ "Logging": { "LogLevel": { "Confi.JsonHttpConfiguration.Source": "Information" }}}'