#!/usr/bin/env bash

TEMPLATES_DIR="templates"
DOCS_DIR="docs"
DATE=$(date +%Y-%m-%d)

if [ -z "$1" ] || [ -z "$2" ]; then
  echo "Usage: ./generate-doc <type> \"Document Name\""
  echo "Types: prd, rfp, tender, architecture, solution, sow, sla, spec, api, data"
  exit 1
fi

TYPE=$1
NAME=$2
TEMPLATE="$TEMPLATES_DIR/$TYPE.md"

if [ ! -f "$TEMPLATE" ]; then
  echo "Template not found: $TEMPLATE"
  exit 1
fi

mkdir -p "$DOCS_DIR"

OUTPUT="$DOCS_DIR/$DATE-$TYPE-${NAME// /_}.md"
cp "$TEMPLATE" "$OUTPUT"

echo "Created: $OUTPUT"
