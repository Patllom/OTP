(function ($) {
    "use strict";

    $(function () {
        var $toggle = $(".mm-toggle");

        $toggle.on("click.siteNavigation", function () {
            var toggle = this;

            window.setTimeout(function () {
                var isOpen = $("body").hasClass("mmPushBody");

                toggle.setAttribute("aria-expanded", String(isOpen));
                toggle.setAttribute("aria-label", isOpen ? "ปิดเมนูหลัก" : "เปิดเมนูหลัก");
            }, 350);
        });

        $(document).on("keydown.siteNavigation", function (event) {
            if (event.key === "Escape" && $toggle.attr("aria-expanded") === "true") {
                event.preventDefault();
                $toggle.trigger("click");
                $toggle.trigger("focus");
            }
        });

        $(window).on("resize.siteNavigation", function () {
            if (window.matchMedia("(min-width: 1366px)").matches) {
                $toggle.attr({
                    "aria-expanded": "false",
                    "aria-label": "เปิดเมนูหลัก"
                });
            }
        });
    });
}(jQuery));
