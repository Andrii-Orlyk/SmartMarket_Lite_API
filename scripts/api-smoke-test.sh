#!/usr/bin/env bash
set -uo pipefail

BASE_URL="${BASE_URL:-http://localhost:5000}"
ADMIN_EMAIL="${ADMIN_EMAIL:-admin@smartmarket.local}"
ADMIN_PASSWORD="${ADMIN_PASSWORD:-Password123!}"

RUN_ID="$(date +%s)-$RANDOM"
USER_EMAIL="buyer-${RUN_ID}@example.com"
USER_PASSWORD="Password123!"
OTHER_EMAIL="other-${RUN_ID}@example.com"
OTHER_TOKEN=""

PASSED=0
FAILED=0
WARNINGS=0

HTTP_BODY=""
HTTP_CODE=""

pass() { echo "PASS: $1"; PASSED=$((PASSED + 1)); }
fail() { echo "FAIL: $1"; FAILED=$((FAILED + 1)); }
warn() { echo "WARN: $1"; WARNINGS=$((WARNINGS + 1)); }

require_commands() {
  command -v curl >/dev/null 2>&1 || { echo "curl is required."; exit 1; }
  command -v jq >/dev/null 2>&1 || { echo "jq is required. Install it with: brew install jq"; exit 1; }
}

api_call() {
  local method="$1"
  local path="$2"
  local data="${3:-}"
  local token="${4:-}"

  local -a args=(-sS -X "$method" "${BASE_URL}${path}" -H "Content-Type: application/json")
  if [[ -n "$token" ]]; then
    args+=(-H "Authorization: Bearer ${token}")
  fi
  if [[ -n "$data" ]]; then
    args+=(-d "$data")
  fi

  local raw
  raw="$(curl "${args[@]}" -w $'\n%{http_code}')"
  HTTP_BODY="$(printf '%s' "$raw" | sed '$d')"
  HTTP_CODE="$(printf '%s' "$raw" | tail -n 1)"
}

expect_status() {
  local name="$1"
  local expected="$2"
  if [[ "$HTTP_CODE" == "$expected" ]]; then
    pass "$name"
    return 0
  fi
  fail "$name (expected HTTP $expected, got $HTTP_CODE)"
  if [[ -n "$HTTP_BODY" ]]; then
    echo "  Response: $HTTP_BODY"
  fi
  return 1
}

echo "SmartMarket Lite API smoke test"
echo "Base URL: $BASE_URL"
echo

require_commands

# 1. Swagger
api_call GET "/swagger/v1/swagger.json"
expect_status "Swagger JSON available" "200"

# 2. Admin auth (development seed or configured credentials)
api_call POST "/api/auth/login" "$(jq -nc --arg e "$ADMIN_EMAIL" --arg p "$ADMIN_PASSWORD" '{email:$e,password:$p}')"
if [[ "$HTTP_CODE" == "200" ]]; then
  ADMIN_TOKEN="$(echo "$HTTP_BODY" | jq -r '.token')"
  pass "Admin login"
else
  fail "Admin login (configure ADMIN_EMAIL/ADMIN_PASSWORD or start API in Development with seed admin)"
  ADMIN_TOKEN=""
fi

# 3-5. User register, login, me
api_call POST "/api/auth/register" "$(jq -nc --arg e "$USER_EMAIL" --arg p "$USER_PASSWORD" '{email:$e,password:$p,firstName:"Jane",lastName:"Buyer"}')"
if [[ "$HTTP_CODE" == "200" ]]; then
  USER_TOKEN="$(echo "$HTTP_BODY" | jq -r '.token')"
  pass "User register"
else
  fail "User register"
  USER_TOKEN=""
fi

api_call POST "/api/auth/login" "$(jq -nc --arg e "$USER_EMAIL" --arg p "$USER_PASSWORD" '{email:$e,password:$p}')"
if [[ "$HTTP_CODE" == "200" ]]; then
  USER_TOKEN="$(echo "$HTTP_BODY" | jq -r '.token')"
  pass "User login"
else
  fail "User login"
fi

api_call GET "/api/auth/me" "" "$USER_TOKEN"
expect_status "Me with token" "200"

# 6-7. Admin product + public list
PRODUCT_ID=""
INITIAL_STOCK=0
if [[ -n "$ADMIN_TOKEN" ]]; then
  PRODUCT_SKU="SMOKE-${RUN_ID}"
  api_call POST "/api/admin/products" "$(jq -nc \
    --arg sku "$PRODUCT_SKU" \
    '{name:"Smoke Mouse",description:"Smoke test product",sku:$sku,price:49.99,stockQuantity:12,isActive:true}')" \
    "$ADMIN_TOKEN"
  if [[ "$HTTP_CODE" == "200" ]]; then
    PRODUCT_ID="$(echo "$HTTP_BODY" | jq -r '.id')"
    INITIAL_STOCK="$(echo "$HTTP_BODY" | jq -r '.stockQuantity')"
    pass "Admin create product"
  else
    fail "Admin create product"
  fi

  api_call GET "/api/products"
  if [[ "$HTTP_CODE" == "200" ]] && echo "$HTTP_BODY" | jq -e --arg id "$PRODUCT_ID" '.items[] | select(.id == $id)' >/dev/null; then
    pass "Public get products"
  else
    fail "Public get products"
  fi
else
  warn "Skipping product/catalog flow (no admin token)"
fi

# 8-10. Cart + checkout
ORDER_ID=""
CART_ITEM_ID=""
if [[ -n "$USER_TOKEN" && -n "$PRODUCT_ID" ]]; then
  api_call POST "/api/cart/items" "$(jq -nc --arg id "$PRODUCT_ID" '{productId:$id,quantity:2}')" "$USER_TOKEN"
  if [[ "$HTTP_CODE" == "200" ]]; then
    CART_ITEM_ID="$(echo "$HTTP_BODY" | jq -r '.items[0].id')"
    pass "Add cart item"
  else
    fail "Add cart item"
  fi

  if [[ -n "$CART_ITEM_ID" && "$CART_ITEM_ID" != "null" ]]; then
    api_call PUT "/api/cart/items/${CART_ITEM_ID}" '{"quantity":3}' "$USER_TOKEN"
    expect_status "Update cart item quantity" "200"
  else
    fail "Update cart item quantity (missing cart item id)"
  fi

  api_call POST "/api/checkout" "" "$USER_TOKEN"
  if [[ "$HTTP_CODE" == "200" ]]; then
    ORDER_ID="$(echo "$HTTP_BODY" | jq -r '.order.id')"
    pass "Checkout"
  else
    fail "Checkout"
  fi

  if [[ -n "$ORDER_ID" && "$ORDER_ID" != "null" ]]; then
    api_call GET "/api/orders/${ORDER_ID}" "" "$USER_TOKEN"
    expect_status "Get order by id" "200"

    api_call GET "/api/orders" "" "$USER_TOKEN"
    if [[ "$HTTP_CODE" == "200" ]] && echo "$HTTP_BODY" | jq -e --arg id "$ORDER_ID" '.[] | select(.id == $id)' >/dev/null; then
      pass "Get order history"
    else
      fail "Get order history"
    fi
  else
    fail "Get order by id (missing order id)"
    fail "Get order history (missing order id)"
  fi

  api_call GET "/api/products/${PRODUCT_ID}"
  if [[ "$HTTP_CODE" == "200" ]]; then
    CURRENT_STOCK="$(echo "$HTTP_BODY" | jq -r '.stockQuantity')"
    if [[ "$CURRENT_STOCK" -eq $((INITIAL_STOCK - 3)) ]]; then
      pass "Product stock decreased"
    else
      fail "Product stock decreased (expected $((INITIAL_STOCK - 3)), got $CURRENT_STOCK)"
    fi
  else
    fail "Product stock decreased (could not read product)"
  fi

  api_call GET "/api/cart" "" "$USER_TOKEN"
  if [[ "$HTTP_CODE" == "200" ]]; then
    ITEM_COUNT="$(echo "$HTTP_BODY" | jq -r '.items | length')"
    if [[ "$ITEM_COUNT" == "0" ]]; then
      pass "Cart cleared after checkout"
    else
      fail "Cart cleared after checkout (items=$ITEM_COUNT)"
    fi
  else
    fail "Cart cleared after checkout"
  fi
else
  warn "Skipping cart/checkout flow (missing user token or product id)"
fi

echo
echo "Negative scenarios:"

# A. Missing JWT
api_call GET "/api/auth/me"
expect_status "Missing JWT on protected endpoint" "401"

# B. Invalid login
api_call POST "/api/auth/login" '{"email":"invalid@example.com","password":"WrongPassword123!"}'
if [[ "$HTTP_CODE" == "401" ]]; then
  CODE="$(echo "$HTTP_BODY" | jq -r '.code // empty')"
  if [[ "$CODE" == "auth.invalid_credentials" ]]; then
    pass "Invalid login"
  else
    fail "Invalid login (expected auth.invalid_credentials, got $CODE)"
  fi
else
  fail "Invalid login (expected HTTP 401, got $HTTP_CODE)"
fi

# C. User forbidden admin product
api_call POST "/api/admin/products" '{"name":"Blocked","sku":"BLOCK-001","price":10,"stockQuantity":1,"isActive":true}' "$USER_TOKEN"
expect_status "User forbidden admin product create" "403"

# D. Invalid product price
if [[ -n "$ADMIN_TOKEN" ]]; then
  api_call POST "/api/admin/products" '{"name":"Bad Price","sku":"BADPRICE-001","price":0,"stockQuantity":1,"isActive":true}' "$ADMIN_TOKEN"
  if [[ "$HTTP_CODE" == "400" ]]; then
    pass "Invalid product price"
  else
    fail "Invalid product price (expected HTTP 400, got $HTTP_CODE)"
  fi
else
  warn "Skipping invalid product price (no admin token)"
fi

# E. Inactive product in cart
if [[ -n "$ADMIN_TOKEN" && -n "$USER_TOKEN" ]]; then
  api_call POST "/api/admin/products" "$(jq -nc --arg sku "INACT-${RUN_ID}" '{name:"Inactive",sku:$sku,price:15,stockQuantity:5,isActive:false}')" "$ADMIN_TOKEN"
  if [[ "$HTTP_CODE" == "200" ]]; then
    INACTIVE_ID="$(echo "$HTTP_BODY" | jq -r '.id')"
    api_call POST "/api/cart/items" "$(jq -nc --arg id "$INACTIVE_ID" '{productId:$id,quantity:1}')" "$USER_TOKEN"
    if [[ "$HTTP_CODE" == "409" ]]; then
      pass "Inactive product cannot be added to cart"
    else
      fail "Inactive product cannot be added to cart (expected HTTP 409, got $HTTP_CODE)"
    fi
  else
    fail "Inactive product setup"
  fi
else
  warn "Skipping inactive product cart test"
fi

# F. Nonexistent product
if [[ -n "$USER_TOKEN" ]]; then
  MISSING_PRODUCT_ID="$(uuidgen 2>/dev/null || python3 -c 'import uuid; print(uuid.uuid4())')"
  api_call POST "/api/cart/items" "$(jq -nc --arg id "$MISSING_PRODUCT_ID" '{productId:$id,quantity:1}')" "$USER_TOKEN"
  if [[ "$HTTP_CODE" == "404" ]]; then
    pass "Add nonexistent product to cart"
  else
    fail "Add nonexistent product to cart (expected HTTP 404, got $HTTP_CODE)"
  fi
fi

# G. Quantity zero
if [[ -n "$USER_TOKEN" && -n "$PRODUCT_ID" ]]; then
  api_call POST "/api/cart/items" "$(jq -nc --arg id "$PRODUCT_ID" '{productId:$id,quantity:0}')" "$USER_TOKEN"
  expect_status "Invalid quantity zero" "400"
fi

# H. Empty checkout
if [[ -n "$USER_TOKEN" ]]; then
  api_call POST "/api/auth/register" "$(jq -nc --arg e "$OTHER_EMAIL" --arg p "$USER_PASSWORD" '{email:$e,password:$p,firstName:"Other",lastName:"User"}')"
  if [[ "$HTTP_CODE" == "200" ]]; then
    OTHER_TOKEN="$(echo "$HTTP_BODY" | jq -r '.token')"
    api_call POST "/api/checkout" "" "$OTHER_TOKEN"
    if [[ "$HTTP_CODE" == "409" ]]; then
      CODE="$(echo "$HTTP_BODY" | jq -r '.code // empty')"
      if [[ "$CODE" == "checkout.empty_cart" ]]; then
        pass "Checkout empty cart"
      else
        fail "Checkout empty cart (expected checkout.empty_cart, got $CODE)"
      fi
    else
      fail "Checkout empty cart (expected HTTP 409, got $HTTP_CODE)"
    fi
  else
    fail "Checkout empty cart setup (register other user)"
  fi
fi

# I. Ownership isolation
if [[ -n "$ORDER_ID" && "$ORDER_ID" != "null" && -n "$OTHER_TOKEN" ]]; then
  api_call GET "/api/orders/${ORDER_ID}" "" "$OTHER_TOKEN"
  if [[ "$HTTP_CODE" == "404" ]]; then
    CODE="$(echo "$HTTP_BODY" | jq -r '.code // empty')"
    if [[ "$CODE" == "order.not_found" ]]; then
      pass "Ownership isolation on order access"
    else
      fail "Ownership isolation (expected order.not_found, got $CODE)"
    fi
  else
    fail "Ownership isolation (expected HTTP 404, got $HTTP_CODE)"
  fi
elif [[ -n "$ORDER_ID" && "$ORDER_ID" != "null" ]]; then
  warn "Skipping ownership isolation (missing other user token)"
fi

# J. Wrong product id
api_call GET "/api/products/00000000-0000-0000-0000-000000000099"
if [[ "$HTTP_CODE" == "404" ]]; then
  pass "Wrong product id returns not found"
else
  fail "Wrong product id (expected HTTP 404, got $HTTP_CODE)"
fi

echo
echo "Summary:"
echo "  Passed: $PASSED"
echo "  Failed: $FAILED"
echo "  Warnings: $WARNINGS"

if [[ "$FAILED" -eq 0 ]]; then
  exit 0
fi

exit 1
