@using RazorLight
@inherits TemplatePage<dynamic>
module Constant_@Model.Id (
    // 输出参数
    value
);

output reg[@Model.Bits:0] value;

always @@(*):
    value = @Model.Bits'H0;

endmodule