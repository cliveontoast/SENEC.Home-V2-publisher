using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities
{
    public class IntervalOfMoments<TMoment> : IIntervalEntity, INotification where TMoment: IIsValid
    {
        public IEnumerable<TMoment> Moments { get; set; }
        public DateTimeOffset IntervalStartIncluded { get; set; }
        public DateTimeOffset IntervalEndExcluded { get; set; }

        public IntervalOfMoments(
            DateTimeOffset intervalStartIncluded,
            DateTimeOffset intervalEndExcluded,
            IEnumerable<TMoment> moments)
        {
            IntervalStartIncluded = intervalStartIncluded;
            IntervalEndExcluded = intervalEndExcluded;
            Moments = moments;
        }
    }
}
