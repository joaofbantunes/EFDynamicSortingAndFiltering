Running a docker container with postgres for tests
docker run -p 5432:5432 --restart unless-stopped --name postgres -e POSTGRES_USER=user -e POSTGRES_PASSWORD=pass -d postgres