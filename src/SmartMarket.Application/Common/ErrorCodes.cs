namespace SmartMarket.Application.Common;

public static class ErrorCodes
{
    public const string ValidationFailed = "validation.failed";
    public const string ServerError = "server.error";

    public const string AuthInvalidCredentials = "auth.invalid_credentials";
    public const string AuthEmailExists = "auth.email_exists";
    public const string AuthUnauthorized = "auth.unauthorized";
    public const string AuthForbidden = "auth.forbidden";

    public const string ProductNotFound = "product.not_found";
    public const string ProductSkuExists = "product.sku_exists";
    public const string ProductInactive = "product.inactive";
    public const string ProductInvalidPrice = "product.invalid_price";
    public const string ProductInvalidStock = "product.invalid_stock";

    public const string CartEmpty = "cart.empty";
    public const string CartItemNotFound = "cart.item_not_found";
    public const string CartInvalidQuantity = "cart.invalid_quantity";

    public const string CheckoutEmptyCart = "checkout.empty_cart";
    public const string CheckoutProductUnavailable = "checkout.product_unavailable";
    public const string CheckoutInsufficientStock = "checkout.insufficient_stock";

    public const string OrderNotFound = "order.not_found";
    public const string OrderInvalidStatusTransition = "order.invalid_status_transition";
}
