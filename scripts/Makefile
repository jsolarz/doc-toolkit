# ============================
# Cross-platform Makefile
# ============================

TEMPLATES_DIR=templates
DOCS_DIR=docs
DATE=$(shell date +%Y-%m-%d)

# Generic document generator
doc:
    @if [ -z "$(type)" ] || [ -z "$(name)" ]; then \
        echo "Usage: make doc type=<prd|rfp|tender|architecture|solution|sow|sla|spec|api|data|blog|weekly-log> name=\"Document Name\""; \
        exit 1; \
    fi; \
    mkdir -p $(DOCS_DIR); \
    SANITIZED_NAME=$$(echo "$(name)" | tr ' ' '_'); \
    cp $(TEMPLATES_DIR)/$(type).md $(DOCS_DIR)/$(DATE)-$(type)-$$SANITIZED_NAME.md; \
    echo "Created: $(DOCS_DIR)/$(DATE)-$(type)-$$SANITIZED_NAME.md"

prd:
    make doc type=prd name="$(name)"

rfp:
    make doc type=rfp name="$(name)"

tender:
    make doc type=tender name="$(name)"

architecture:
    make doc type=architecture name="$(name)"

solution:
    make doc type=solution name="$(name)"

sow:
    make doc type=sow name="$(name)"

list:
    @echo "Available templates:"; \
    ls $(TEMPLATES_DIR)

clean:
    rm -rf $(DOCS_DIR)/*
    echo "Docs folder cleaned."
