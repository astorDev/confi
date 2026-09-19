TEST ?= main
ARGS ?= -r --hero=Batman

run:
	dotnet run -- $(ARGS)

kill:
	kill `lsof -t -i:27645`

yac:
	httpyac send $(TEST).http --all

play:
	bash -c -i 'make run & sleep 5 && make yac && kill -INT %1'

apply-v:
	cp Versions/$(V)/Program.cs Program.cs

stash-v:
	mkdir -p Versions/$(V) && cp Program.cs Versions/$(V)/Program.cs

play-with-v:
	make apply-v
	make play

test:
	dotnet test --filter FullyQualifiedName~Confi.Clis.Playground.$(NAME) --logger "console;verbosity=detailed"