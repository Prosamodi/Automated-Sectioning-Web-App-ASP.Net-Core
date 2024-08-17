FROM mcr.microsoft.com/dotnet/aspnet:7.0

COPY /bin/Release/net7.0/publish/ AutoSect.App/

RUN apt-get update && apt-get install -y ca-certificates

COPY certs/AspNetLocalCertificate.pfx /certs/AspNetLocalCertificate.pfx

RUN update-ca-certificates

ENV ASPNETCORE_ENVIRONMENT Production
ENV Logging__Console__FormatterName=Simple

EXPOSE 4001
WORKDIR /AutoSect.App
ENTRYPOINT ["dotnet", "AutoSect.App.dll",  "--urls=https://0.0.0.0:4001"]