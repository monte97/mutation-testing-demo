FROM mcr.microsoft.com/dotnet/sdk:10.0

WORKDIR /app

# Stryker.NET 5.0.0 e ReportGenerator, fissati nel tool manifest locale
COPY .config/dotnet-tools.json .config/
RUN dotnet tool restore

COPY MutationTestingDemo.sln ./
COPY src/MutationTestingDemo/MutationTestingDemo.csproj src/MutationTestingDemo/
COPY tests/MutationTestingDemo.Tests/MutationTestingDemo.Tests.csproj tests/MutationTestingDemo.Tests/

RUN dotnet restore

COPY . .

RUN dotnet build -c Debug

ENTRYPOINT ["bash"]
