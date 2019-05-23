namespace XTinyLog
{
    internal enum LogTypeEnum
    {
        Info,
        Warning,
        Debug,
        Error
    }

    internal class Constants
    {

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public const string StyleTag =
                @"<style>
            html, body {
                font-family: Verdana,sans-serif;
                font-size: 15px;
                line-height: 1.5;
            }

            .lbl {
                margin-left: 15px;
            }

            .highlight {
                background: #FFFF00
            }

            .info {
                color: #4e4e4e;
                background-color: #c5cac3;
                border-color: #282928;
            }

            .alert {
                padding: 5px;
                margin-bottom: 3px;
                /*border: 1px solid transparent;*/
                border-radius: 4px;
            }

            .warning {
                color: #8a6d3b;
                background-color: #fcf8e3;
                border-color: #faebcc;
            }

            .error {
                color: #a94442;
                background-color: #f2dede;
                border-color: #ebccd1;
            }

            .debug {
                color: #3c763d;
                background-color: #dff0d8;
                border-color: #d6e9c6;
            }
           </style>";

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public const string SearchDivTag =
            @"<div>
                <h3> File Date : 12/78/789</h3>

                <label class='lbl'> Type </label>
                <select id='logType' onchange='drowLogData();'>
                    <option value ='all' > All </ option >
                    <option value ='info'>Info</option>
                    <option value ='warning'> Warning </option>
                    <option value ='debug'>Debug</option>
                    <option value ='error'> Error </option>
                </select>

              <label class='lbl'> Sort</label>
              <select id='logSort' onchange='drowLogData();'>
                    <option value ='desc' > Desc </option>
                    <option value='ase'>Ase</option>
              </select>

              <label class='lbl'> Search</label>
              <input type = 'text' id='logSearch' onkeyup='drowLogData();' />

              <input type = 'button' id='logSearch' oninput='clearSearch();'  value='Clear' />
            </div> <hr />";

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public const string BeginScriptTag =
            @"<script>
                 drowLogData();

                function drowLogData() {
                    var logData = logs();

                    var container = document.getElementById('container');
                    container.innerHTML = '';

                    if (logData.length <= 0) return;

                    var isAse = document.getElementById('logSort').value == 'ase';
                    var startIndex = isAse ? 0 : logData.length - 1;
                    var endIndex = isAse ? logData.length - 1: 0;
                    var logType = document.getElementById('logType').value;
                    var logSearch = document.getElementById('logSearch').value;

                    for (var i = startIndex; (isAse ? i <= endIndex : i >= endIndex); i = ((isAse) ? i + 1 : i - 1)) {
                        var row = getEligibleElement(logData[i],logType,logSearch);
                        if (!row) continue;
                        container.appendChild(row);
                    }
                }

                function getEligibleElement(data, logType, logSearch) {
                    if (!(logType == 'all' || logType == data.type)) return null;

                    var errorMessage = data.message;
                    if (logSearch) {
                        var reg = new RegExp('(' + logSearch + ')', 'ig');
                        if (!reg.test(errorMessage)) return null;
                        errorMessage = errorMessage.replace(reg, '<span class=\'highlight\'>$1</span>');
                    }

                    var div = document.createElement('div');
                    div.className = 'alert ' + data.type;

                    div.innerHTML = data.time + ' :: ' + errorMessage;

                    return div;
                }

                function clearSearch() {
                    document.getElementById('logSearch').value = '';
                    document.getElementById('logSort').value = 'desc';
                    document.getElementById('logType').value = 'all';
                    drowLogData();
                }

                function logs() {
                    return [
                            {
                       ";

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public const string EngTags = "   ]; } </script>  </body></html>";

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public const string BeginLogHtmlFile = 
            "<!DOCTYPE html><html lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\"> \n <head>";

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public const string MetaTag = " <meta charset=\"utf-8\" />";

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public const string TitleTag = "<title>{0}</title>";

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public const string BeginBodyTag = "</head> <body>";

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public const string ContainerDivTag = "<div id='container'> </div>";
    }
}