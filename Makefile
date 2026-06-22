# Makefile for XIVThirdEye Plugin
# Usage: make build               - Build and deploy the plugin
#        make package             - Create release zip
#        make package RELEASE_TAG=v1.0.0 - Release zip with updated download links
#        make clean               - Clean build artifacts

PROJECT_NAME = XIVThirdEye
CSPROJ = $(PROJECT_NAME).csproj
DLL_NAME = $(PROJECT_NAME).dll
JSON_NAME = $(PROJECT_NAME).json
YAML_NAME = $(PROJECT_NAME).yaml

BUILD_DIR = bin/Debug
BUILD_DLL = $(BUILD_DIR)/$(DLL_NAME)
BUILD_JSON = $(BUILD_DIR)/$(JSON_NAME)

SOURCE_JSON = $(JSON_NAME)

PLUGIN_DIR = ~/Library/Application\ Support/XIV\ on\ Mac/dalamud/Hooks/dev/plugins
PLUGIN_DLL = $(PLUGIN_DIR)/$(DLL_NAME)
PLUGIN_JSON = $(PLUGIN_DIR)/$(JSON_NAME)

CONFIGURATION = Debug

.PHONY: all build clean deploy rebuild build-only package info help

all: build

build: $(BUILD_DLL) deploy
	@echo "✅ Build and deployment complete!"
	@echo "   DLL: $(BUILD_DLL)"
	@echo "   Installed to: $(PLUGIN_DIR)"

CS_FILES := $(shell find . -name '*.cs' -not -path './obj/*')

$(BUILD_DLL): $(CSPROJ) $(CS_FILES)
	@echo "🔨 Building $(PROJECT_NAME)..."
	dotnet build -c $(CONFIGURATION)
	@test -f $(BUILD_DLL) || (echo "❌ Build failed - DLL not found" && exit 1)

$(BUILD_JSON): $(SOURCE_JSON)
	@cp $(SOURCE_JSON) $(BUILD_JSON)

deploy: $(BUILD_DLL) $(PLUGIN_DIR)
	@echo "📦 Deploying plugin files..."
	@cp $(BUILD_DLL) $(PLUGIN_DLL)
	@cp $(SOURCE_JSON) $(PLUGIN_JSON)
	@echo "   ✅ Copied $(DLL_NAME)"
	@echo "   ✅ Copied $(JSON_NAME)"

$(PLUGIN_DIR):
	@echo "📁 Creating plugin directory..."
	@mkdir -p $(PLUGIN_DIR)

clean:
	@echo "🧹 Cleaning build artifacts..."
	dotnet clean
	@echo "✅ Clean complete"

rebuild: clean build

build-only:
	@echo "🔨 Building only (no deployment)..."
	dotnet build -c $(CONFIGURATION)

# Package for release. Bumps AssemblyVersion in all three manifest files.
# Usage: make package [RELEASE_TAG=v1.0.0]
package: $(BUILD_DLL) $(BUILD_JSON)
	@echo "📦 Creating release package..."
	@TIMESTAMP=$$(date +%s); \
	if command -v jq >/dev/null 2>&1; then \
		CURRENT_VERSION=$$(jq -r '.[0].AssemblyVersion' repo.json); \
		MAJOR=$$(echo $$CURRENT_VERSION | cut -d. -f1); \
		MINOR=$$(echo $$CURRENT_VERSION | cut -d. -f2); \
		PATCH=$$(echo $$CURRENT_VERSION | cut -d. -f3); \
		BUILD=$$(echo $$CURRENT_VERSION | cut -d. -f4); \
		NEW_BUILD=$$((BUILD + 1)); \
		NEW_VERSION="$$MAJOR.$$MINOR.$$PATCH.$$NEW_BUILD"; \
		REPO_URL=$$(jq -r '.[0].RepoUrl' repo.json); \
		if [ -n "$$RELEASE_TAG" ]; then \
			DOWNLOAD_URL="$$REPO_URL/releases/download/$$RELEASE_TAG/$(PROJECT_NAME).zip"; \
			jq --arg ts $$TIMESTAMP --arg ver $$NEW_VERSION --arg url $$DOWNLOAD_URL \
				'.[0].LastUpdate = ($$ts | tonumber) | .[0].AssemblyVersion = $$ver | .[0].DownloadLinkInstall = $$url | .[0].DownloadLinkUpdate = $$url | .[0].DownloadLinkTesting = $$url' \
				repo.json > repo.json.tmp && mv repo.json.tmp repo.json; \
			echo "✅ repo.json: version=$$NEW_VERSION, LastUpdate=$$TIMESTAMP, links=$$RELEASE_TAG"; \
		else \
			jq --arg ts $$TIMESTAMP --arg ver $$NEW_VERSION \
				'.[0].LastUpdate = ($$ts | tonumber) | .[0].AssemblyVersion = $$ver' \
				repo.json > repo.json.tmp && mv repo.json.tmp repo.json; \
			echo "✅ repo.json: version=$$NEW_VERSION, LastUpdate=$$TIMESTAMP"; \
			echo "⚠️  DownloadLinks unchanged. Run: make package RELEASE_TAG=v1.0.1 to update them"; \
		fi; \
		jq --arg ver $$NEW_VERSION '.AssemblyVersion = $$ver' $(JSON_NAME) > $(JSON_NAME).tmp && mv $(JSON_NAME).tmp $(JSON_NAME); \
		echo "✅ $(JSON_NAME): version synced to $$NEW_VERSION"; \
		sed -i.bak "s/\"AssemblyVersion\": \"[^\"]*\"/\"AssemblyVersion\": \"$$NEW_VERSION\"/" $(YAML_NAME) && rm -f $(YAML_NAME).bak; \
		echo "✅ $(YAML_NAME): version synced to $$NEW_VERSION"; \
	else \
		echo "❌ jq is required for 'make package'. Install with: brew install jq"; \
		exit 1; \
	fi
	@mkdir -p dist
	@rm -f dist/$(PROJECT_NAME).zip
	@cd $(BUILD_DIR) && \
		([ -f $(DLL_NAME) ] && zip -q ../../dist/$(PROJECT_NAME).zip $(DLL_NAME) || (echo "❌ $(DLL_NAME) not found" && exit 1)) && \
		([ -f $(DLL_NAME:.dll=.deps.json) ] && zip -q ../../dist/$(PROJECT_NAME).zip $(DLL_NAME:.dll=.deps.json) || echo "⚠️  deps.json not found") && \
		cd ../.. && \
		([ -f $(JSON_NAME) ] && zip -q dist/$(PROJECT_NAME).zip $(JSON_NAME) || (echo "❌ $(JSON_NAME) not found" && exit 1)) && \
		([ -f $(YAML_NAME) ] && zip -q dist/$(PROJECT_NAME).zip $(YAML_NAME) || (echo "❌ $(YAML_NAME) not found" && exit 1)) && \
		([ -f thirdeye.png ] && zip -q dist/$(PROJECT_NAME).zip thirdeye.png || echo "⚠️  thirdeye.png not found")
	@if [ -f dist/$(PROJECT_NAME).zip ]; then \
		echo "✅ Package created: dist/$(PROJECT_NAME).zip"; \
		ls -lh dist/$(PROJECT_NAME).zip; \
		echo ""; \
		echo "Package contents:"; \
		unzip -l dist/$(PROJECT_NAME).zip | grep -E "\.(dll|json|yaml)$$" || echo "  (checking contents...)"; \
	else \
		echo "❌ Package creation failed"; \
		exit 1; \
	fi

info:
	@echo "Plugin Information:"
	@echo "  Project: $(PROJECT_NAME)"
	@echo "  Build DLL: $(BUILD_DLL)"
	@echo "  Plugin Directory: $(PLUGIN_DIR)"
	@echo ""
	@if [ -f $(BUILD_DLL) ]; then \
		ls -lh $(BUILD_DLL); \
	else \
		echo "  ❌ DLL not built yet. Run 'make build' first."; \
	fi

help:
	@echo "XIVThirdEye Plugin Makefile"
	@echo ""
	@echo "Available targets:"
	@echo "  make build                       - Build and deploy plugin (default)"
	@echo "  make build-only                  - Build without deploying"
	@echo "  make deploy                      - Deploy files (after building)"
	@echo "  make package                     - Create release zip (bumps version)"
	@echo "  make package RELEASE_TAG=v1.0.0  - Release zip with updated download links"
	@echo "  make clean                       - Clean build artifacts"
	@echo "  make rebuild                     - Clean and rebuild"
	@echo "  make info                        - Show plugin file information"
	@echo "  make help                        - Show this help message"
	@echo ""
	@echo "Plugin will be installed to:"
	@echo "  $(PLUGIN_DIR)"
