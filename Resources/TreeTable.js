var TreeTable = (function (_super) {
    __extends(TreeTable, _super);
    function TreeTable() {
        return _super !== null && _super.apply(this, arguments) || this;
    }

    TreeTable.prototype.createContent = function () {

        var self = this;
        var page = Forguncy.Page;

        var container = $("<div id='" + this.ID + "'></div>");
        var innerContainer = $(`<table class="layui-table layui-form" id="` + this.ID + `t" lay-size='sm'"></table>`);
        container.append(innerContainer);

        //在页面数据加载完成后执行
        page.bind("PageDefaultDataLoaded", function () {
            var element = self.CellElement;
            var cellTypeMetaData = element.CellType;
            var setBindingListViewParam = cellTypeMetaData.SetBindingListViewParam;
            //获取要展示数据的json对象数组
            var listViewData = self.getListViewData(setBindingListViewParam);
            //获取要展示列的json对象数组
            var colums = self.getColums(setBindingListViewParam);
            //填充树形表格
            self.fillingTable(self, listViewData, colums);
        })

        return container;
    };

    TreeTable.prototype.getListViewData = function (ListViewParam) {

		var listViewName = ListViewParam.ListViewName;
		var id = ListViewParam.ID;
		var relatedParentID = ListViewParam.RelatedParentID;
		var fieldInfos = ListViewParam.MyFieldInfos;
		var fields = fieldInfos.map((info) => { return info.Field; });
        var names = fieldInfos.map((info) => { return info.Name; });

        //即使不显示ID与关联ID列，仍要在构造对象时拥有这两个属性的数据
		fields.push(id);
		fields.push(relatedParentID);

		var page = Forguncy.Page;
		var listView = page.getListView(listViewName);
		var listViewData = new Array();
		for (var i = 0; i < listView.getRowCount(); i++) {
            var obj = {};
			for (var j = 0; j < fields.length; j++) {
				var queryField = "" + fields[j];
				if (queryField === "Button") {
					obj[queryField + j] = "<button rowIndex=" + i + " colIndex=" + j + ">" + names[j] + "</button>";
                } else if (queryField === "Hyperlink") {
                    if (fieldInfos[j].HyperlinkTemplate.IsBold == null) {
                        var font_weight = "normal";
                    } else {
                        var font_weight = fieldInfos[j].HyperlinkTemplate.IsBold ? "bold" : "normal";
                    }
                    //获取背景色和前景色
                    var background = Forguncy.ConvertToCssColor(fieldInfos[j].HyperlinkTemplate.BackgroundColor);
                    var color = Forguncy.ConvertToCssColor(fieldInfos[j].HyperlinkTemplate.FrontColor);
                    obj[queryField + j] = "<a href='#' style='font-weight:" + font_weight + ";background:" + background + ";color:" + color + "' rowIndex=" + i + " colIndex=" + j + ">" + names[j] + "</a>";
				} else {
					obj[queryField] = listView.getText(i, queryField);
				}
            }
            obj["ListViewRowIndex"] = i;
			listViewData.push(obj);
		}
		return listViewData;
	};

    TreeTable.prototype.getColums = function (ListViewParam) {
        var fieldInfos = ListViewParam.MyFieldInfos;
        var colums = new Array();
        for (var i = 0; i < fieldInfos.length; i++) {
            var colum = {};
            colum["key"] = fieldInfos[i].Field;
            colum["title"] = fieldInfos[i].Name;
            colum["width"] = "100px";
            colum["align"] = "center";
            if (fieldInfos[i].Type != 0) {
                let Field = fieldInfos[i].Field + i;
                colum["template"] = function (item) {
                    var field = Field;
                    return item[field];
                }
            }
            colums.push(colum);
        }
        return colums;
    }

    TreeTable.prototype.fillingTable = function (self, listViewData, colums) {
        layui.use(['treeTable', 'layer', 'code', 'form'], function () {
            var o = layui.$,
                form = layui.form,
                layer = layui.layer,
                treeTable = layui.treeTable;
            //渲染生成表格对象
            var re = treeTable.render({
                //innerContainerde的id
                elem: '#' + self.ID + 't',
                //数据使用json对象数组
                data: listViewData,
                //展开图标展示列
                icon_key: '名称',
                //主键
                primary_key: 'ID',
                //关联主键
                parent_key: '关联父ID',
                //是否显示checkbox列
                is_checkbox: false,
                end: function (e) {
                    form.render();
                },
                //列设置
                cols: colums
            });

            self.setUnfolding(treeTable, re);
            self.decorateTable();
            self.bindEvents();
            // 监听展开关闭
            //treeTable.on('tree(flex)', function (data) {
            //    layer.msg(JSON.stringify(data));
            //});
            // 监听checkbox选择
            //treeTable.on('tree(box)', function (data) {
            //    if (o(data.elem).parents('#tree-table1').length) {
            //        var text = [];
            //        o(data.elem).parents('#tree-table1').find('.cbx.layui-form-checked').each(function () {
            //            o(this).parents('[data-pid]').length && text.push(o(this).parents('td').next().find('span').text());
            //        });
            //        o(data.elem).parents('#tree-table1').prev().find('input').val(text.join(','));
            //    }
            //});
            // 监听自定义
            //treeTable.on('tree(click)', function (data) {
            //    //layer.msg(JSON.stringify(data));
            //});
            // 获取选中值，返回值是一个数组（定义的primary_key参数集合）
            //o('.get-checked').click(function () {
            //    layer.msg('选中参数' + treeTable.checked(re).join(','));
            //});
            // 刷新重载树表（一般在异步处理数据后刷新显示）
            //o('.refresh').click(function () {
                //re.data.push({ "id": 50, "pid": 0, "title": "1-4" }, { "id": 51, "pid": 50, "title": "1-4-1" });
                //treeTable.render(re);
            //});
            //o('#tree1').on('click', '[data-down]', function () {
            //    o(this).find('span').length && o(this).parents('.layui-unselect').find('input').val(o(this).text());
            //});
            //o('.layui-select-title').click(function () {
            //    o(this).parent().hasClass('layui-form-selected') ? o(this).next().hide() : o(this).next().show(), o(this).parent().toggleClass('layui-form-selected');
            //});
            //o(document).on("click", function (i) {
            //    !o(i.target).parent().hasClass('layui-select-title') && !o(i.target).parents('table').length && !(!o(i.target).parents('table').length && o(i.target).hasClass('layui-icon')) && o(".layui-form-select").removeClass("layui-form-selected").find('.layui-anim').hide();
            //});
        });
    }

    TreeTable.prototype.setUnfolding = function (treeTable, re) {
        var cellTypeMetaData = this.CellElement.CellType;
        var unfoldingMode = cellTypeMetaData.SetUnfoldingMode;

        //0：全部收起； 1：全部展开 2：展开到指定级别
        if (unfoldingMode === 0) {
            treeTable.closeAll(re);
        } else if (unfoldingMode == 1) {
            treeTable.openAll(re);
        } else {
            //通过设置节点级别小于展开级别的节点ID实现分级展开
            var unfoldingLevel = cellTypeMetaData.UnfoldingLevel;
            var res = "";
            for (var i = 0; i < re.data.length; i++) {
                if (re.data[i].level < unfoldingLevel) {
                    res += "," + re.data[i].ID;
                }
            }
            localStorage.setItem(re.elem.substr(1), res.substr(1));
            treeTable.render(re);
        }
    }
    
    TreeTable.prototype.decorateTable = function () {
        var id = "#" + this.ID;
        var self = this;
        var listViewName = self.CellElement.CellType.SetBindingListViewParam.ListViewName;
        //触发点击事件后切换样式并选中listview的对应行
        $(id + "t").on("click", "tr", function () {
            if (!$(this).hasClass("selected")) {
                $(".selected").not(this).removeClass("selected");
                $(this).toggleClass("selected");
            }
            var index = this.getAttribute("listviewindex");
            Forguncy.Page.getListView(listViewName).selectRow(index);
            self.CellElement.Value = this.dataset.id;
            self.commitValue();
        });

        $(id).css('overflow', 'auto');
        $(id).css('height', "100%");
        //添加单元格样式
        this.addStyle();
        if (this.CellElement.CellType.GridLineShow) {
            //绘制网格线
            this.printGrid();
        }
        //为单元格预设值添加上选中样式
        $("tr[data-id=" + self.CellElement.Value + "]").addClass("selected");
        Forguncy.Page.getListView(listViewName).selectRow($("tr[data-id=" + self.CellElement.Value + "]").attr("listviewindex"));
    };

    TreeTable.prototype.addStyle = function () {
        var id = "#" + this.ID;
        $(id + "t thead").addClass("TreeTableTreeTable-" + this.CellElement.CellType.TemplateKey + "-tableHead");
        $(id + "t tbody tr").addClass("TreeTableTreeTable-" + this.CellElement.CellType.TemplateKey + "-tableBody");
    };
    
    TreeTable.prototype.printGrid = function () {
        var id = "#" + this.ID;
        var color = Forguncy.ConvertToCssColor(this.CellElement.CellType.GridLineColor);
        var width = this.CellElement.CellType.GridLineWidth;

        $(id + "t").css("border", width + "px solid " + color);
        //仅设置上、右框线，以避免内部框线被加粗
        $(id + "t th").css("border-width", "0px " + width + "px " + width + "px 0px");
        $(id + "t td").css("border-width", "0px " + width + "px " + width + "px 0px");
        $(id + "t th").css("border-style", "solid");
        $(id + "t td").css("border-style", "solid");
        $(id + "t th").css("border-color", color);
        $(id + "t td").css("border-color", color);
    };

    TreeTable.prototype.getValueFromElement = function () {
        return this.CellElement.Value;
    };

    TreeTable.prototype.setValueToElement = function (element, value) {
        if (this.CellElement.Value !== value) {
            this.CellElement.Value = value;
        }
    };

    TreeTable.prototype.bindEvents = function () {

        var self = this;
        var cellTypeMetaData = this.CellElement.CellType
        var listViewName = cellTypeMetaData.SetBindingListViewParam.ListViewName;
        var listView = Forguncy.Page.getListView(listViewName);
        //当表格重新加载，就重加载页面树型表
        listView.bind("reloaded", function () {
            Forguncy.DelayRefresh.Push(self, function () {
                Forguncy.DelayRefresh.Push(self, function () {
                    self.reloadData();
                }, "jQueryTreeTable_reloaded1");
            }, "jQueryTreeTable_reloaded2");
        });

        //绑定按钮和超链接执行命令
        var fieldInfos = cellTypeMetaData.SetBindingListViewParam.MyFieldInfos;
        var fields = fieldInfos.map((info) => { return info.Field; });

        for (let i = 0; i < fields.length; i++) {
            if (fields[i] === "Button") {
                $("button[colIndex=" + i + "]").on("click", function (commandList) {
                    return function (event) {
                        self.executeCommand(commandList);
                        event.preventDefault();
                    };
                }(fieldInfos[i].CommandList));
            } else if (fields[i] === "Hyperlink") {
                $("a[colIndex=" + i + "]").on("click", function (commandList) {
                    return function () {
                        self.executeCommand(commandList);
                    };
                }(fieldInfos[i].CommandList));
            }
        }
    };

    TreeTable.prototype.reloadData = function () {
        var self = this;
        var container = $("#" + self.ID);
        container.empty();
        var cellTypeMetaData = self.CellElement.CellType;
        var listViewData = self.getListViewData(cellTypeMetaData.SetBindingListViewParam);
        var colums = self.getColums(cellTypeMetaData.SetBindingListViewParam);
        var innerContainer = self.fillingTable(self, listViewData, colums, cellTypeMetaData);
        container.append(innerContainer);
    };

    TreeTable.prototype.disable = function () {
        _super.prototype.disable.call(this);
    };

    TreeTable.prototype.enable = function () {
        _super.prototype.enable.call(this);
    };

    return TreeTable;
}(Forguncy.CellTypeBase));

// Key format is "Namespace.ClassName, AssemblyName"
Forguncy.Plugin.CellTypeHelper.registerCellType("TreeTable.TreeTable, TreeTable", TreeTable);

