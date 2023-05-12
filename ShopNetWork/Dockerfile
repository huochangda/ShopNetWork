#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 81

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["ShopNetWork/ShopNetWork.csproj", "ShopNetWork/"]
COPY ["Model/Model.csproj", "Model/"]
COPY ["ShopNet.Core/ShopNet.Core.csproj", "ShopNet.Core/"]
COPY ["ShopNet.Common/ShopNet.Common.csproj", "ShopNet.Common/"]
COPY ["ShopNetWork.Interfaces/ShopNetWork.Interfaces.csproj", "ShopNetWork.Interfaces/"]
RUN dotnet restore "ShopNetWork/ShopNetWork.csproj"
COPY . .
WORKDIR "/src/ShopNetWork"
RUN dotnet build "ShopNetWork.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ShopNetWork.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShopNetWork.dll"]