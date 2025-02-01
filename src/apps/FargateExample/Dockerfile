FROM mcr.microsoft.com/dotnet/aspnet:8.0

RUN apt-get update
RUN apt-get install --yes curl

WORKDIR /app
COPY . .
EXPOSE 80
ENTRYPOINT ["dotnet", "Acme.FargateExample.dll"]