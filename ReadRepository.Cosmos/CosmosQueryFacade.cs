// copied as is from  https://github.com/jeremylindsayni/AzureCosmos_Sample/blob/master/CosmosEmulatorSample_Part2/CosmosQueryFacade.cs
/*
MIT License

Copyright(c) 2019 Jeremy Lindsay

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace ReadRepository.DocumentDB
{
    public class CosmosQueryFacade<T> where T : class
    {
        public string CollectionId { get; set; }

        public string DatabaseId { get; set; }

        public DocumentClient DocumentClient { get; set; }

        public async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate)
        {
            var documentCollectionUrl = UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId);

            var query = DocumentClient.CreateDocumentQuery<T>(documentCollectionUrl)
                .Where(predicate)
                .AsDocumentQuery();

            var results = new List<T>();

            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }

            return results;
        }
    }
}
