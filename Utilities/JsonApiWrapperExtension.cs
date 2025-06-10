namespace tnki_line_sale_api.Utilities
{
    public static class JsonApiWrapperExtension
    {
        public static IApplicationBuilder UseJsonApiWrapper(this IApplicationBuilder app)
        {
            return app.UseMiddleware<JsonApiWrapper>();
        }
    }
}
