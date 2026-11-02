using AshproSAASERP.Models.Enums;
using AshproSAASERP.Models.Others;
using AshproSAASERP.Shared.Services;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace AshproSAASERP.Shared.Pages.Others;

public partial class PrintDesignerView : ITabAware, IAsyncDisposable
{
    #region Varibles
    [Parameter] public int TabId { get; set; }
    byte[]? pdfData;
    bool isLoadingPreview = false;
    int activeTabIndex = 0;
    PrintDesignHdr hdr = new();
    PrintDesignDtls dtls = new();
    List<string> FormType = ["Invoice", "Voucher", "Report"];
    string[] PrintType = { "A4", "Thermal" };
    string[] AlignmentList = { "Left", "Center", "Right" };
    List<string> FontStyle = ["Bold", "Italic", "Regular", "Underline", "Strikeout"];
    string[] FontFamilies =
    {
        "Helvetica",
        "ZapfDingbats",
        "TimesRoman",
        "Symbol",
        "Courier"
    };
    string? sPosition, sControleType, sControl, _searchString;
    List<string> ControlType = ["LabelControl", "ValueControl", "HorizontalDotedLine", "HorizontalLine", "VerticalLine", "Image"];
    List<string> listControl = new List<string>();
    List<string> ControlTypeList = new();
    List<PrintDesignDtls> Controls = new();
    PrintDesignDtls? dragTarget;
    double startX, startY;
    double originX, originY;
    PrintDesignDtls? editTarget;
    bool IsEditing = false;

    HashSet<PrintDesignDtls> SelectedControls = new();
    Dictionary<PrintDesignDtls, (int X, int Y)> dragOrigins = new();
    DotNetObjectReference<PrintDesignerView>? _dotNetRef;
    int nudgeStep = 1;
    bool isMarqueeSelecting = false;
    double marqueeStartX, marqueeStartY;
    double marqueeCurrentX, marqueeCurrentY;
    #endregion

    (string Justify, string TextAlign) GetAlignmentCss(string alignment)
    => alignment.ToLower() switch
    {
        "left" => ("flex-start", "left"),
        "right" => ("flex-end", "right"),
        "center" => ("center", "center"),
        _ => ("center", "center")   // default
    };
    protected override async Task OnInitializedAsync()
    {
        ControlTypeList = ControlType;
        hdr.FormType = "Invoice";
        hdr.VoucherType = InvoiceTypes.Sales_Invoice.ToString();
        hdr.PrintType = "A4";
        hdr.PageWidth = 700;
        hdr.PageHeight = 1000;
        hdr.SpaceBtwnLines = 10;
        hdr.LinesPerPage = 40;
        await base.OnInitializedAsync();
    }
    string GetStyle(PrintDesignDtls c)
    {
        bool isSelected = SelectedControls.Contains(c) || (SelectedControls.Count == 0 && dtls == c);
        // border condition
        var isShape =
            c.ControlType?.Equals("HorizontalLine", StringComparison.OrdinalIgnoreCase) == true ||
            c.ControlType?.Equals("VerticalLine", StringComparison.OrdinalIgnoreCase) == true;
        var border = isSelected
            ? "2px solid #2563eb"
            : (isShape
                ? $"{(c.FontSize == 0 ? 1 : c.FontSize)}px solid #000"
                : "1px dashed #777");
        var selectedBg = isSelected ? "background-color:rgba(37,99,235,0.08);" : "";
        var selectedShadow = isSelected ? "box-shadow:0 0 0 2px rgba(37,99,235,0.25); outline:1px solid #2563eb;" : "";
        var fontSize = (c.FontSize == 0 ? 12 : c.FontSize);
        var fontFamily = string.IsNullOrWhiteSpace(c.Font) ? "Arial" : c.Font;
        string fontWeight = "normal";
        string fontStyle = "normal";
        string textDecoration = "none";

        var styles = (c.FStyle ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var s in styles)
        {
            switch (s.Trim())
            {
                case "Bold":
                    fontWeight = "bold";
                    break;
                case "Italic":
                    fontStyle = "italic";
                    break;
                case "Underline":
                    textDecoration = "underline";
                    break;
                case "Strikeout":
                    textDecoration = "line-through";
                    break;
            }
        }
        var alignment = GetAlignmentCss(c.Alignment ?? "center");
        return
            $"position:absolute;" +
            $"left:{c.PositionX}px;" +
            $"top:{c.PositionY}px;" +
            $"width:{c.Width}px;" +
            $"height:{c.Height}px;" +
            $"border:{border};" +
            $"{selectedBg}" +
            $"{selectedShadow}" +
            $"font-size:{(c.FontSize == 0 ? 12 : c.FontSize)}px;" +
            $"font-style:{fontStyle};" +
            $"font-weight:{fontWeight};" +
            $"text-decoration:{textDecoration};" +
            $"font-family:{(string.IsNullOrWhiteSpace(c.Font) ? "Arial" : c.Font)};" +
            $"display:flex;" +
            $"align-items:center;" +
            $"justify-content:{alignment.Justify};" +
            $"text-align:{alignment.TextAlign};" +
            $"user-select:none;" +
            $"cursor:move;";
    }
    void OnDtlsChanged()
    {
        if (!IsEditing || string.IsNullOrEmpty(dtls.ControlName)) { return; }
        dragTarget = dtls;
        StateHasChanged();
    }
    void SelectControl(MouseEventArgs? e, PrintDesignDtls c)
    {
        if (e != null && (e.CtrlKey || e.ShiftKey))
        {
            if (SelectedControls.Contains(c))
                SelectedControls.Remove(c);
            else
                SelectedControls.Add(c);
        }
        else
        {
            if (!SelectedControls.Contains(c) || SelectedControls.Count <= 1)
            {
                SelectedControls.Clear();
                SelectedControls.Add(c);
            }
        }
        dtls = c;
        StateHasChanged();
    }

    void SelectAll()
    {
        SelectedControls.Clear();
        foreach (var c in Controls)
        {
            SelectedControls.Add(c);
        }
        if (Controls.Any())
        {
            dtls = Controls.First();
        }
        StateHasChanged();
    }

    void ClearSelection()
    {
        SelectedControls.Clear();
        StateHasChanged();
    }

    void MoveSelected(int deltaX, int deltaY)
    {
        var targets = SelectedControls.Count > 0 
            ? SelectedControls.ToList() 
            : (dtls != null && Controls.Contains(dtls) ? new List<PrintDesignDtls> { dtls } : new List<PrintDesignDtls>());

        if (!targets.Any()) return;

        foreach (var item in targets)
        {
            item.PositionX = Math.Max(0, item.PositionX + deltaX);
            item.PositionY = Math.Max(0, item.PositionY + deltaY);
        }

        StateHasChanged();
    }

    void AlignLeft()
    {
        if (SelectedControls.Count <= 1) return;
        int minX = SelectedControls.Min(c => c.PositionX);
        foreach (var c in SelectedControls)
        {
            c.PositionX = minX;
        }
        StateHasChanged();
    }

    void AlignRight()
    {
        if (SelectedControls.Count <= 1) return;
        int maxRight = SelectedControls.Max(c => c.PositionX + c.Width);
        foreach (var c in SelectedControls)
        {
            c.PositionX = maxRight - c.Width;
        }
        StateHasChanged();
    }

    void AlignTop()
    {
        if (SelectedControls.Count <= 1) return;
        int minY = SelectedControls.Min(c => c.PositionY);
        foreach (var c in SelectedControls)
        {
            c.PositionY = minY;
        }
        StateHasChanged();
    }

    void AlignBottom()
    {
        if (SelectedControls.Count <= 1) return;
        int maxBottom = SelectedControls.Max(c => c.PositionY + c.Height);
        foreach (var c in SelectedControls)
        {
            c.PositionY = maxBottom - c.Height;
        }
        StateHasChanged();
    }

    void OnCanvasClick(MouseEventArgs e)
    {
        if (!e.CtrlKey && !e.ShiftKey && !isMarqueeSelecting)
        {
            SelectedControls.Clear();
            StateHasChanged();
        }
    }

    void OnCanvasMouseDown(MouseEventArgs e)
    {
        if (dragTarget != null) return;
        if (!e.CtrlKey && !e.ShiftKey)
        {
            SelectedControls.Clear();
        }
        isMarqueeSelecting = true;
        marqueeStartX = e.OffsetX;
        marqueeStartY = e.OffsetY;
        marqueeCurrentX = e.OffsetX;
        marqueeCurrentY = e.OffsetY;
        StateHasChanged();
    }

    void OnCanvasMouseMove(MouseEventArgs e)
    {
        if (!isMarqueeSelecting) return;
        marqueeCurrentX = e.OffsetX;
        marqueeCurrentY = e.OffsetY;
        StateHasChanged();
    }

    void OnCanvasMouseUp(MouseEventArgs e)
    {
        if (!isMarqueeSelecting) return;
        isMarqueeSelecting = false;

        double left = Math.Min(marqueeStartX, marqueeCurrentX);
        double right = Math.Max(marqueeStartX, marqueeCurrentX);
        double top = Math.Min(marqueeStartY, marqueeCurrentY);
        double bottom = Math.Max(marqueeStartY, marqueeCurrentY);

        if (right - left > 4 || bottom - top > 4)
        {
            foreach (var c in Controls)
            {
                if (c.PositionX + c.Width >= left && c.PositionX <= right &&
                    c.PositionY + c.Height >= top && c.PositionY <= bottom)
                {
                    SelectedControls.Add(c);
                }
            }
            if (SelectedControls.Any())
            {
                dtls = SelectedControls.First();
            }
        }
        StateHasChanged();
    }
    private void OnFormTypeChanged(string formType)
    {
        hdr.FormType = formType;
    }
    private async Task OnChangeVoucherType(string voucherType)
    {
        hdr.VoucherType = voucherType;
        if (voucherType == InvoiceTypes.Sales_Invoice_Thermal.ToString())
        {
            hdr.PrintType = "Thermal";
            hdr.PageWidth = 290;
            hdr.PageHeight = 30000;
            hdr.SpaceBtwnLines = 5;
            hdr.LinesPerPage = 100;
        }
        else if (voucherType == InvoiceTypes.Sales_Invoice.ToString())
        {
            hdr.PrintType = "A4";
            hdr.PageWidth = 700;
            hdr.PageHeight = 1000;
            hdr.SpaceBtwnLines = 10;
            hdr.LinesPerPage = 40;
        }
        await LoadDefaultTemplate();
    }
    private void OnPrintTypeChanged(string printType)
    {
        hdr.PrintType = printType;
        if (printType == "Thermal")
        {
            hdr.PageWidth = 290;
            if (hdr.PageHeight < 1000) hdr.PageHeight = 30000;
            if (hdr.LinesPerPage < 50) hdr.LinesPerPage = 100;
            hdr.SpaceBtwnLines = 5;
        }
        else if (printType == "A4")
        {
            hdr.PageWidth = 700;
            hdr.PageHeight = 1000;
            hdr.LinesPerPage = 40;
            hdr.SpaceBtwnLines = 10;
        }
        pdfData = null;
        StateHasChanged();
    }
    private async Task LoadDefaultTemplate()
    {
        try
        {
            bool isThermal = hdr.PrintType == "Thermal" || hdr.VoucherType == InvoiceTypes.Sales_Invoice_Thermal.ToString();
            string voucher = hdr.VoucherType ?? (isThermal ? InvoiceTypes.Sales_Invoice_Thermal.ToString() : InvoiceTypes.Sales_Invoice.ToString());
            var req = new PrintDesignReq(0, voucher, isThermal);
            var dto = await api.GetPrintDesignAsync(req);
            if (dto?.hdr != null && dto.dtls != null && dto.dtls.Any())
            {
                hdr = dto.hdr;
                Controls = dto.dtls;
                if (Controls.Any())
                {
                    dtls = Controls.First();
                }
                pdfData = null;
                StateHasChanged();
                return;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading default print design from API: {ex.Message}");
        }

        try
        {
            bool isThermal = hdr.PrintType == "Thermal" || hdr.VoucherType == InvoiceTypes.Sales_Invoice_Thermal.ToString();
            var fallback = PrintDesignSampleData.GetFallbackDesign(hdr.VoucherType ?? "Sales_Invoice", isThermal);
            if (fallback?.hdr != null && fallback.dtls != null && fallback.dtls.Any())
            {
                hdr = fallback.hdr;
                Controls = fallback.dtls;
                if (Controls.Any())
                {
                    dtls = Controls.First();
                }
                pdfData = null;
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading fallback design: {ex.Message}");
        }
    }
    private void OnPositionChanged(string position)
    {
        sPosition = position;
        switch (position)
        {
            case "Invoice_Section":
            case "Company_Section":
            case "Party_Section":
            case "Footer_Section":
            case "Footer_Bottom":
            case "Footer_Details":
            case "Report_Header":
            case "Report_Footer_Section":
                ControlTypeList = ControlType;
                break;
            case "Invoice_Details_Header":
            case "Report_Details_Header":
                ControlTypeList = ["LabelControl", "HorizontalDotedLine", "HorizontalLine", "VerticalLine"];
                break;
            case "Details_Area":
            case "Report_Details_Area":
                ControlTypeList = ["ValueControl"];
                break;
        }
    }
    private void OnControlTypeChanged(string cType)
    {
        sControleType = cType;
        if (cType == "ValueControl")
        {
            if (hdr.FormType == "Invoice")
            {
                switch (sPosition)
                {
                    case "Company_Section":
                        var company = new CompanyDetails();
                        listControl = company.GetProperties();
                        break;
                    case "Footer_Section":
                    case "Footer_Bottom":
                    case "Invoice_Section":
                        var invoice = new TransactionHdr();
                        listControl = invoice.GetProperties();
                        break;
                    case "Party_Section":
                        var party = new Party();
                        listControl = party.GetProperties();
                        break;
                    case "Details_Area":
                        var trans = new TransactionDtls();
                        listControl = trans.GetProperties();
                        break;
                    case "Footer_Details":
                        var fDetails = new FooterDetails();
                        listControl = fDetails.GetProperties();
                        break;
                    default:
                        break;
                }
            }
            else if (hdr.FormType == "Voucher")
            {
                string typeName = $"AshproSAASERP.Models.DTO.{hdr.VoucherType.Replace("_", "")}VchrDtls,AshproSAASERP.Models";
                if (sPosition != "Report_Details_Area")
                {
                    typeName = $"AshproSAASERP.Models.DTO.{hdr.VoucherType.Replace("_", "")}VchrHdr,AshproSAASERP.Models";
                }
                var type = Type.GetType(typeName);
                listControl = type?.GetProperties().Select(p => p.Name).ToList()!;
            }
            else
            {
                string typeName = $"AshproSAASERP.Models.DTO.{hdr.VoucherType.Replace("_", "")}Dtls,AshproSAASERP.Models";
                if (sPosition != "Report_Details_Area")
                {
                    typeName = $"AshproSAASERP.Models.DTO.{hdr.VoucherType.Replace("_", "")}Hdr,AshproSAASERP.Models";
                }
                var type = Type.GetType(typeName);
                listControl = type?.GetProperties().Select(p => p.Name).ToList()!;
            }
        }
    }
    private void OnValueChanged(string value)
    {
        sControl = value;

    }
    int GetPositionY()
    {
        return sPosition switch
        {
            "Company_Section" => 50,
            "Party_Section" => 100,
            "Invoice_Section" => 150,
            "Invoice_Details_Header" => 250,
            "Details_Area" => 300,
            "Footer_Section" => 900,
            "Footer_Bottom" => 900,
            "Footer_Details" => 900,
            "Report_Header" => 80,
            "Report_Details_Header" => 900,
            "Report_Details_Area" => 900,
            "Report_Footer_Section" => 900,
            _ => 50,
        };
    }
    int GetPositionX()
    {
        return sControleType switch
        {
            "LabelControl" => 50,
            "ValueControl" => 100,
            "HorizontalDotedLine" => 10,
            "HorizontalLine" => 30,
            "VerticalLine" => 150,
            _ => 50,
        };
    }
    private void AddControl()
    {
        var index = Controls.Count(x => x.ControlType == sControleType) + 1;
        var name = $"{sControleType}_{index}";

        // base defaults
        var pdtls = new PrintDesignDtls
        {
            ControlType = sControleType!,
            FieldText = "",
            PositionX = GetPositionX(),
            PositionY = GetPositionY(),
            Width = 120,
            Height = 20,
            IsBold = true,
            FontSize = 10,
            ControlName = name,
            PIndex = Controls.Count,
            Location = sPosition!,
            Alignment = "Left",
            Font = "TimesRoman",
            Spec = ""
        };

        switch (sControleType)
        {
            case "LabelControl":
                pdtls.FieldText = sControl!;
                break;

            case "ValueControl":
                pdtls.FieldText = sControl!;
                pdtls.Width = 140;
                break;

            case "HorizontalDotedLine":
            case "HorizontalLine":
                pdtls.Width = 740;
                pdtls.Height = 2;
                pdtls.FontSize = 1;
                break;

            case "VerticalLine":
                pdtls.Width = 2;
                pdtls.Height = 965;
                pdtls.FontSize = 1;
                break;
            case "QRCode":
            case "Barcode":
            case "Image":
                pdtls.Width = 200;
                pdtls.Height = 200;
                pdtls.FontSize = 1;
                break;
        }

        Controls.Add(pdtls);
        dtls = pdtls;
    }
    void StartDrag(MouseEventArgs e, PrintDesignDtls c)
    {
        if (e.CtrlKey || e.ShiftKey)
        {
            if (SelectedControls.Contains(c))
                SelectedControls.Remove(c);
            else
                SelectedControls.Add(c);
        }
        else
        {
            if (!SelectedControls.Contains(c))
            {
                SelectedControls.Clear();
                SelectedControls.Add(c);
            }
        }

        dtls = c;
        dragTarget = c;
        startX = e.ClientX;
        startY = e.ClientY;
        originX = c.PositionX;
        originY = c.PositionY;

        dragOrigins.Clear();
        foreach (var item in SelectedControls)
        {
            dragOrigins[item] = (item.PositionX, item.PositionY);
        }

        _dotNetRef ??= DotNetObjectReference.Create(this);
        _ = JS.InvokeVoidAsync("designer.attach", _dotNetRef);
    }
    [JSInvokable]
    public void OnMouseMove(double x, double y)
    {
        if (dragTarget is null) return;
        int deltaX = (int)(x - startX);
        int deltaY = (int)(y - startY);

        if (SelectedControls.Count > 1 && dragOrigins.Count > 0)
        {
            foreach (var item in SelectedControls)
            {
                if (dragOrigins.TryGetValue(item, out var orig))
                {
                    item.PositionX = Math.Max(0, orig.X + deltaX);
                    item.PositionY = Math.Max(0, orig.Y + deltaY);
                }
            }
        }
        else
        {
            dragTarget.PositionX = Math.Max(0, (int)(originX + deltaX));
            dragTarget.PositionY = Math.Max(0, (int)(originY + deltaY));
        }

        dtls = dragTarget;
        StateHasChanged();
    }
    [JSInvokable]
    public void OnMouseUp()
    {
        dragTarget = null;
        dragOrigins.Clear();
    }

    [JSInvokable]
    public void HandleKeyDownFromJs(string key, bool shiftKey, bool ctrlKey, bool altKey)
    {
        int step = shiftKey ? (nudgeStep > 1 ? nudgeStep * 2 : 5) : nudgeStep;

        switch (key)
        {
            case "ArrowUp":
            case "Up":
                MoveSelected(0, -step);
                break;
            case "ArrowDown":
            case "Down":
                MoveSelected(0, step);
                break;
            case "ArrowLeft":
            case "Left":
                MoveSelected(-step, 0);
                break;
            case "ArrowRight":
            case "Right":
                MoveSelected(step, 0);
                break;
            case "a":
            case "A":
                if (ctrlKey)
                {
                    SelectAll();
                }
                break;
            case "Escape":
                ClearSelection();
                break;
        }
    }

    void HandleKeyDown(KeyboardEventArgs e)
    {
        int step = e.ShiftKey ? (nudgeStep > 1 ? nudgeStep * 2 : 5) : nudgeStep;

        switch (e.Key)
        {
            case "ArrowUp":
            case "Up":
                MoveSelected(0, -step);
                break;
            case "ArrowDown":
            case "Down":
                MoveSelected(0, step);
                break;
            case "ArrowLeft":
            case "Left":
                MoveSelected(-step, 0);
                break;
            case "ArrowRight":
            case "Right":
                MoveSelected(step, 0);
                break;
            case "a":
            case "A":
                if (e.CtrlKey)
                {
                    SelectAll();
                }
                break;
            case "Escape":
                ClearSelection();
                break;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetRef ??= DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("designer.registerKeyboard", _dotNetRef);
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await JS.InvokeVoidAsync("designer.unregisterKeyboard");
        }
        catch { }
        _dotNetRef?.Dispose();
    }
    RenderFragment RenderControl(PrintDesignDtls c) => builder =>
    {
        switch (c.ControlType?.Trim().ToLower())
        {
            case "LabelControl":
            case "ValueControl":
                builder.OpenComponent(0, typeof(MudText));
                builder.AddAttribute(1, "ChildContent",
                    (RenderFragment)(b => b.AddContent(2, c.FieldText ?? "")));
                builder.CloseComponent();
                break;
            case "HorizontalDotedLine":
            case "HorizontalLine":
                builder.OpenElement(3, "div");
                builder.AddAttribute(4, "style",
                    "width:100%; height:1px; background:black;");
                builder.CloseElement();
                break;

            case "VerticalLine":
                builder.OpenElement(5, "div");
                builder.AddAttribute(6, "style",
                    "width:1px; height:100%; background:black;");
                builder.CloseElement();
                break;
        }
    };
    async Task SearchDesign()
    {
        var result = await Dialog.TransactionSearchMethod("Print Design");
        if (result is not null)
        {
            var dto = await api.GetPrintDesignAsync(new PrintDesignReq(result.Id, "pdesign"));
            hdr = dto.hdr;
            Controls = dto.dtls;
            StateHasChanged();
        }
    }
    async Task RowClicked(DataGridRowClickEventArgs<PrintDesignDtls> args)
    {
        dtls = args.Item;
        if (!SelectedControls.Contains(dtls))
        {
            SelectedControls.Clear();
            SelectedControls.Add(dtls);
        }
        StateHasChanged();
    }
    async Task DeleteItem(PrintDesignDtls? item = null)
    {
        if (item == null) { item = dtls; }
        if (item == null) { return; }
        if (await Dialog.DeleteDialogMethod())
        {
            if (SelectedControls.Count > 1 && SelectedControls.Contains(item))
            {
                foreach (var sc in SelectedControls.ToList())
                {
                    Controls.Remove(sc);
                }
                SelectedControls.Clear();
                dtls = Controls.FirstOrDefault() ?? new PrintDesignDtls();
            }
            else
            {
                Controls.Remove(item);
                SelectedControls.Remove(item);
                dtls = Controls.FirstOrDefault() ?? new PrintDesignDtls();
            }
            StateHasChanged();
        }
    }
    async Task SaveOrUpdate()
    {
        try
        {
            if (string.IsNullOrEmpty(hdr.DesignName) || !Controls.Any()) { return; }
            hdr.FK_BranchId = AppData.BranchId;
            var dto = new PrintDesignDTO(hdr, Controls);
            if (hdr.PrintDesignHdrId == 0)
            {
                var output = await api.CreatePrintDesignAsync(dto);
                if (string.IsNullOrEmpty(output.Message))
                {
                    Dialog.DialogMethod(Localizer("SaveMessage"));
                    NewClick();
                }
                else
                {
                    Dialog.DialogMethod(output.Message);
                }
            }
            else
            {
                var output = await api.UpdatePrintDesignAsync(dto);
                if (string.IsNullOrEmpty(output.Message))
                {
                    Dialog.DialogMethod(Localizer("UpdateMessage"));
                    NewClick();
                }
                else
                {
                    Dialog.DialogMethod(output.Message);
                }
            }
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod(ex.Message);
        }
    }
    async Task Delete()
    {
        if (!Controls.Any() || hdr.PrintDesignHdrId == 0) { return; }
        if (await Dialog.DeleteDialogMethod())
        {
            var output = await api.DeletePrintDesignAsync(hdr.PrintDesignHdrId);
            if (string.IsNullOrEmpty(output.Message))
            {
                NewClick();
            }
            else
            {
                Dialog.DialogMethod(output.Message);
            }
        }
    }
    void NewClick()
    {
        Controls.Clear();
        SelectedControls.Clear();
        dtls = new PrintDesignDtls();
        dragTarget = new PrintDesignDtls();
        pdfData = null;
        activeTabIndex = 0;
        StateHasChanged();
    }
    async Task UploadFiles(IBrowserFile file)
    {
        try
        {
            // basic validation
            if (file == null)
                throw new Exception("No file selected.");

            // ensure JSON file
            if (file.ContentType != "application/json" &&
                !file.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Please upload a valid JSON file.");
            }

            using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);

            var dto = await JsonSerializer.DeserializeAsync<PrintDesignDTO>(stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (dto is null)
                throw new Exception("Invalid JSON format.");

            hdr = dto.hdr;
            hdr.PrintDesignHdrId = 0;
            Controls = dto.dtls ?? new List<PrintDesignDtls>();
            Controls.ForEach(c => c.PrintDesignDtlsId = 0);
            StateHasChanged();
        }
        catch (JsonException)
        {
            Dialog.DialogMethod("The uploaded file is not valid JSON.");
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod($"{ex.Message}_427");
        }
    }
    async Task DownloadJson()
    {
        try
        {
            if (hdr == null || string.IsNullOrEmpty(hdr.DesignName) || !Controls.Any()) { return; }
            var obj = new PrintDesignDTO(hdr, Controls);

            // serialize to JSON (pretty formatted)
            var json = JsonSerializer.Serialize(
                obj,
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

            // convert string → bytes
            var data = Encoding.UTF8.GetBytes(json);

            // download in browser
            await downloader.DownloadFromStream(data, $"{hdr.DesignName}.json");
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod($"{ex.Message}_453");
        }
    }

    async Task PreviewDesign()
    {
        try
        {
            isLoadingPreview = true;
            activeTabIndex = 1;
            StateHasChanged();

            if (!Controls.Any())
            {
                await LoadDefaultTemplate();
            }

            if (!Controls.Any())
            {
                Dialog.DialogMethod("No controls available to preview. Please add or load controls first.");
                isLoadingPreview = false;
                StateHasChanged();
                return;
            }
            bool isThermal = hdr.PrintType == "Thermal";
            var sampleDto = PrintDesignSampleData.CreateSampleInvoiceData(hdr, Controls, isThermal);

            if (isThermal)
            {
                var thermalHelper = new SyncfusionPrintHelperThermal();
                pdfData = await thermalHelper.PrintMethod(downloader, sampleDto, 1, null, isPreview: true);
            }
            else
            {
                var pHelper = new SyncfusionPrintHelper();
                pdfData = await pHelper.PrintMethod(downloader, sampleDto, 1, null, isPreview: true);
            }
        }
        catch (Exception ex)
        {
            Dialog.DialogMethod($"Preview error: {ex.Message}");
        }
        finally
        {
            isLoadingPreview = false;
            StateHasChanged();
        }
    }

    async Task OpenPreviewDialog()
    {
        if (pdfData is not null)
        {
            await Dialog.InvoicePreviewDialog(pdfData);
        }
    }

    async Task DownloadPreviewPdf()
    {
        if (pdfData is not null)
        {
            string filename = $"{hdr.VoucherType ?? "Invoice"}_Preview_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            await downloader.DownloadFromStream(pdfData, filename);
        }
    }

    string GetCanvasStyle()
    {
        bool isThermal = hdr.PrintType == "Thermal" || hdr.VoucherType == InvoiceTypes.Sales_Invoice_Thermal.ToString();
        int width = isThermal ? (hdr.PageWidth > 0 ? hdr.PageWidth : 290) : 794;
        int height = isThermal ? Math.Max(1200, Controls.Any() ? Controls.Max(c => c.PositionY + c.Height) + 100 : 1200) : 1123;
        return $"position:relative;width:{width}px;min-height:{height}px;background-color:#ffffff;" +
               "background-image:linear-gradient(to right, rgba(0,0,0,0.06) 1px, transparent 1px), linear-gradient(to bottom, rgba(0,0,0,0.06) 1px, transparent 1px);background-size:50px 50px;margin:0 auto;box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1);border:1px solid #ddd;";
    }

}
