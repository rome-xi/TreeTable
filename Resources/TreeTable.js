var TreeTable = (function (_super) {
    __extends(TreeTable, _super);
    function TreeTable() {
        return _super !== null && _super.apply(this, arguments) || this;
    }

    TreeTable.prototype.createContent = function () {
        var self = this;

		var page = Forguncy.Page;
        var element = this.CellElement;
		var cellTypeMetaData = element.CellType;

        page.bind("PageDefaultDataLoaded", function () {
            var listViewData = self.getListViewData(cellTypeMetaData.SetBindingListViewParam);
            var colums = self.getColums(cellTypeMetaData.SetBindingListViewParam);
            self.fillingTable(self, listViewData, colums, cellTypeMetaData);
		})

        var container = $("<div id='" + this.ID + "'></div>");

        var innerContainer = $(`<table class="layui-table layui-form" id="` + this.ID +`t" lay-size='sm'"></table>`);

        container.append(innerContainer);

        return container;
    };

	TreeTable.prototype.getListViewData = function (ListViewParam) {
		var listViewName = ListViewParam.ListViewName;
		var id = ListViewParam.ID;
		var relatedParentID = ListViewParam.RelatedParentID;
		var fieldInfos = ListViewParam.MyFieldInfos;
		var fields = fieldInfos.map((info) => { return info.Field; });
		var names = fieldInfos.map((info) => { return info.Name; });
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

    TreeTable.prototype.fillingTable = function (self, listViewData, colums, cellTypeMetaData) {
        layui.use(['treeTable', 'layer', 'code', 'form'], function () {
            var o = layui.$,
                form = layui.form,
                layer = layui.layer,
                treeTable = layui.treeTable;
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
                checked: {
                    key: 'ID',
                    data: [1, 4, 5, 2, 6, 3],
                },
                end: function (e) {
                    form.render();
                },
                //列设置
                cols: colums
            });
            // 监听展开关闭
            treeTable.on('tree(flex)', function (data) {
                //layer.msg(JSON.stringify(data));
            });
            // 监听checkbox选择
            treeTable.on('tree(box)', function (data) {
                if (o(data.elem).parents('#tree-table1').length) {
                    var text = [];
                    o(data.elem).parents('#tree-table1').find('.cbx.layui-form-checked').each(function () {
                        o(this).parents('[data-pid]').length && text.push(o(this).parents('td').next().find('span').text());
                    });
                    o(data.elem).parents('#tree-table1').prev().find('input').val(text.join(','));
                }
            });
            // 监听自定义
            treeTable.on('tree(click)', function (data) {
                //layer.msg(JSON.stringify(data));
            });
            // 获取选中值，返回值是一个数组（定义的primary_key参数集合）
            o('.get-checked').click(function () {
                layer.msg('选中参数' + treeTable.checked(re).join(','));
            });
            // 刷新重载树表（一般在异步处理数据后刷新显示）
            o('.refresh').click(function () {
                //re.data.push({ "id": 50, "pid": 0, "title": "1-4" }, { "id": 51, "pid": 50, "title": "1-4-1" });
                //treeTable.render(re);
            });
            // 随机更换小图标
            o('.change-icon').click(function () {
                var arr = [
                    {
                        open: 'layui-icon layui-icon-set',
                        close: 'layui-icon layui-icon-set-fill',
                        left: 16,
                    },
                    {
                        open: 'layui-icon layui-icon-rate',
                        close: 'layui-icon layui-icon-rate-solid',
                        left: 16,
                    },
                    {
                        open: 'layui-icon layui-icon-tread',
                        close: 'layui-icon layui-icon-praise',
                        left: 16,
                    },
                    {
                        open: 'layui-icon layui-icon-camera',
                        close: 'layui-icon layui-icon-camera-fill',
                        left: 16,
                    },
                    {
                        open: 'layui-icon layui-icon-user',
                        close: 'layui-icon layui-icon-group',
                        left: 16,
                    },
                ];
                var round = Math.round(Math.random() * (arr.length - 1));
                re.icon = arr[round];
                treeTable.render(re);
            });
            o('#tree1').on('click', '[data-down]', function () {
                o(this).find('span').length && o(this).parents('.layui-unselect').find('input').val(o(this).text());
            });
            o('.layui-select-title').click(function () {
                o(this).parent().hasClass('layui-form-selected') ? o(this).next().hide() : o(this).next().show(), o(this).parent().toggleClass('layui-form-selected');
            });
            o(document).on("click", function (i) {
                !o(i.target).parent().hasClass('layui-select-title') && !o(i.target).parents('table').length && !(!o(i.target).parents('table').length && o(i.target).hasClass('layui-icon')) && o(".layui-form-select").removeClass("layui-form-selected").find('.layui-anim').hide();
            });


            var unfoldingMode = cellTypeMetaData.SetUnfoldingMode;
            if (unfoldingMode === 0) {
                treeTable.closeAll(re);
            } else if (unfoldingMode == 1) {
                treeTable.openAll(re);
            } else {
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

            self.decorateTable();
            self.bindEvents(cellTypeMetaData);
        });
    }
    
    TreeTable.prototype.decorateTable = function () {
        var id = "#" + this.ID;
        var self = this;
        var listViewName = self.CellElement.CellType.SetBindingListViewParam.ListViewName;

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
        this.addStyle(id);
        if (this.CellElement.CellType.GridLineShow) {
            this.printGrid(id);
        }

        $("tr[data-id=" + self.CellElement.Value + "]").addClass("selected");
        Forguncy.Page.getListView(listViewName).selectRow($("tr[data-id=" + self.CellElement.Value + "]").attr("listviewindex"));
    };

    TreeTable.prototype.addStyle = function (id) {
        $(id + "t thead").addClass("TreeTableTreeTable-" + this.CellElement.CellType.TemplateKey + "-tableHead");
        $(id + "t tbody tr").addClass("TreeTableTreeTable-" + this.CellElement.CellType.TemplateKey + "-tableBody");
    };

    TreeTable.prototype.printGrid = function (id) {
        var color = Forguncy.ConvertToCssColor(this.CellElement.CellType.GridLineColor);
        var width = this.CellElement.CellType.GridLineWidth;
        //var headColor = Forguncy.ConvertToCssColor(this.CellElement.StyleTemplate.Styles.tableHead.NormalStyle.Background);
        $(id + "t").css("border", width + "px solid " + color);
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

    TreeTable.prototype.bindEvents = function (cellTypeMetaData) {
        var self = this;
        var listViewName = cellTypeMetaData.SetBindingListViewParam.ListViewName;
        var listView = Forguncy.Page.getListView(listViewName);
        listView.bind("reloaded", function () {
            Forguncy.DelayRefresh.Push(self, function () {
                Forguncy.DelayRefresh.Push(self, function () {
                    self.reloadData();
                }, "jQueryTreeTable_reloaded1");
            }, "jQueryTreeTable_reloaded2");
        });


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
        var unfoldingMethod = cellTypeMetaData.SetUnfoldingMethod;
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

