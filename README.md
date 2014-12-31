MyQRCode
========
这是我的本科毕业设计：基于加密的二维码编码解码的小软件

摘要
全国代码管理中心为了提高代码信息化服务水平，提高代码业务处理效率，需要建立一套新的组织机构代码业务信息采集系统。在目前仍然使用的旧系统当中，组织机构信息的识别、录入、管理依然是人工方式，存在着效率低下，人力成本高，容易出错的问题，也存在着信息易泄漏、易伪造的安全隐患。
本文结合这个项目的实际需求，对该系统中的组织机构代码自动识别子系统进行了设计和实现，旨在解决旧系统中上述的种种问题。本文对二维码的编码解码技术进行了探究和分析，对其中的中文识别存在的问题进行了解决，以此来实现高效的组织机构信息的识别、录入；并对文本加密进行了探究，选择了几种有代表性的算法进行了分析，结合他们的优点缺点，尝试对其进行了结合和改进，以此来解决信息易泄漏、易伪造的安全隐患；对于加密后出现的新问题——本文长度太长，不利于二维码的生成和解码，本文为此也研究了字符串压缩，并作相应结合，以解决二维码容量有限的问题；为了实现高效信息管理目的，本文也对二维码批量生成、搜索功能进行了实现，为此，本文对文本多串模糊匹配，多线程同步、异步操作等相关算法和技术进行了探究，并作相应实现和结合。
该子系统在完成后，通过使用二维码存储信息的方式达到了组织机构信息高效高质量识别、录入的目的；通过加密实现了关键信息需要安全隐藏、不能被伪造的功能；虽然在加密后出现了文本长度过长的问题，但通过文本压缩的方式得到了解决；对于二维码信息的管理，该系统对组织机构的多项信息进行格式化，实现了快速的批量生成、搜索功能，为海量信息的管理提供了接口。
关键字：自动识别，二维码，加密，文本压缩，多串匹配