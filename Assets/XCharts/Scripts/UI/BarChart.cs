﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XCharts
{
    [AddComponentMenu("XCharts/BarChart", 14)]
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class BarChart : CoordinateChart
    {
        private float m_BarLastOffset = 0;
        private HashSet<string> m_SerieNameSet = new HashSet<string>();
        private Dictionary<int, List<Serie>> m_StackSeries = new Dictionary<int, List<Serie>>();
        private List<float> m_SeriesCurrHig = new List<float>();

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            m_Title.text = "BarChart";
            m_Tooltip.type = Tooltip.Type.Shadow;
            RemoveData();
            AddSerie("serie1", SerieType.Bar);
            for (int i = 0; i < 5; i++)
            {
                AddXAxisData("x" + (i + 1));
                AddData(0, Random.Range(10, 90));
            }
        }
#endif

        private void DrawYBarSerie(VertexHelper vh, int serieIndex, int stackCount,
            Serie serie, int colorIndex, ref List<float> seriesHig)
        {
            if (!IsActive(serie.name)) return;
            var xAxis = m_XAxises[serie.axisIndex];
            var yAxis = m_YAxises[serie.axisIndex];
            if (!yAxis.show) yAxis = m_YAxises[(serie.axisIndex + 1) % m_YAxises.Count];

            float categoryWidth = yAxis.GetDataWidth(coordinateHig, m_DataZoom);
            float barGap = GetBarGap();
            float totalBarWidth = GetBarTotalWidth(categoryWidth, barGap);
            float barWidth = serie.GetBarWidth(categoryWidth);
            float offset = (categoryWidth - totalBarWidth) / 2;
            float barGapWidth = barWidth + barWidth * barGap;
            float space = serie.barGap == -1 ? offset : offset + m_BarLastOffset;

            var showData = serie.GetDataList(m_DataZoom);
            int maxCount = maxShowDataNumber > 0 ?
                (maxShowDataNumber > showData.Count ? showData.Count : maxShowDataNumber)
                : showData.Count;
            if (seriesHig.Count < minShowDataNumber)
            {
                for (int i = 0; i < minShowDataNumber; i++)
                {
                    seriesHig.Add(0);
                }
            }
            for (int i = minShowDataNumber; i < maxCount; i++)
            {
                if (i >= seriesHig.Count)
                {
                    seriesHig.Add(0);
                }
                float value = showData[i].data[1];
                float pX = seriesHig[i] + coordinateX + xAxis.zeroXOffset + yAxis.axisLine.width;
                float pY = coordinateY + +i * categoryWidth;
                if (!yAxis.boundaryGap) pY -= categoryWidth / 2;
                float barHig = (xAxis.minValue > 0 ? value - xAxis.minValue : value)
                    / (xAxis.maxValue - xAxis.minValue) * coordinateWid;
                seriesHig[i] += barHig;
                Vector3 p1 = new Vector3(pX, pY + space + barWidth);
                Vector3 p2 = new Vector3(pX + barHig, pY + space + barWidth);
                Vector3 p3 = new Vector3(pX + barHig, pY + space);
                Vector3 p4 = new Vector3(pX, pY + space);
                var highlight = (m_Tooltip.show && m_Tooltip.IsSelected(i))
                    || serie.data[i].highlighted
                    || serie.highlighted;
                if (serie.show)
                {
                    Color areaColor = serie.GetAreaColor(m_ThemeInfo, colorIndex, highlight);
                    Color areaToColor = serie.GetAreaToColor(m_ThemeInfo, colorIndex, highlight);
                    ChartHelper.DrawPolygon(vh, p1, p2, p3, p4, areaColor, areaToColor);
                }
            }
            if (!m_Series.IsStack(serie.stack, SerieType.Bar))
            {
                m_BarLastOffset += barGapWidth;
            }
        }

        private void DrawXBarSerie(VertexHelper vh, int serieIndex, int stackCount,
            Serie serie, int colorIndex, ref List<float> seriesHig)
        {
            if (!IsActive(serie.name)) return;
            var showData = serie.GetDataList(m_DataZoom);
            var yAxis = m_YAxises[serie.axisIndex];
            var xAxis = m_XAxises[serie.axisIndex];
            if (!xAxis.show) xAxis = m_XAxises[(serie.axisIndex + 1) % m_XAxises.Count];
            float categoryWidth = xAxis.GetDataWidth(coordinateWid, m_DataZoom);
            float barGap = GetBarGap();
            float totalBarWidth = GetBarTotalWidth(categoryWidth, barGap);
            float barWidth = serie.GetBarWidth(categoryWidth);
            float offset = (categoryWidth - totalBarWidth) / 2;
            float barGapWidth = barWidth + barWidth * barGap;
            float space = serie.barGap == -1 ? offset : offset + m_BarLastOffset;
            int maxCount = maxShowDataNumber > 0 ?
                (maxShowDataNumber > showData.Count ? showData.Count : maxShowDataNumber)
                : showData.Count;

            if (seriesHig.Count < minShowDataNumber)
            {
                for (int i = 0; i < minShowDataNumber; i++)
                {
                    seriesHig.Add(0);
                }
            }
            for (int i = minShowDataNumber; i < maxCount; i++)
            {
                if (i >= seriesHig.Count)
                {
                    seriesHig.Add(0);
                }
                float value = showData[i].data[1];
                float pX = coordinateX + i * categoryWidth;
                float zeroY = coordinateY + yAxis.zeroYOffset;
                if (!xAxis.boundaryGap) pX -= categoryWidth / 2;
                float pY = seriesHig[i] + zeroY + xAxis.axisLine.width;
                float barHig = (yAxis.minValue > 0 ? value - yAxis.minValue : value)
                    / (yAxis.maxValue - yAxis.minValue) * coordinateHig;
                seriesHig[i] += barHig;

                Vector3 p1 = new Vector3(pX + space, pY);
                Vector3 p2 = new Vector3(pX + space, pY + barHig);
                Vector3 p3 = new Vector3(pX + space + barWidth, pY + barHig);
                Vector3 p4 = new Vector3(pX + space + barWidth, pY);
                serie.dataPoints.Add(new Vector3(pX + space + barWidth / 2, pY + barHig));
                var highlight = (m_Tooltip.show && m_Tooltip.IsSelected(i))
                    || serie.data[i].highlighted
                    || serie.highlighted;

                if (serie.show)
                {
                    Color areaColor = serie.GetAreaColor(m_ThemeInfo, colorIndex, highlight);
                    Color areaToColor = serie.GetAreaToColor(m_ThemeInfo, colorIndex, highlight);
                    ChartHelper.DrawPolygon(vh, p4, p1, p2, p3, areaColor, areaToColor);
                }
            }
            if (!m_Series.IsStack(serie.stack, SerieType.Bar))
            {
                m_BarLastOffset += barGapWidth;
            }
        }

        private float GetBarGap()
        {
            float gap = 0.3f;
            for (int i = 0; i < m_Series.Count; i++)
            {
                var serie = m_Series.series[i];
                if (serie.type == SerieType.Bar)
                {
                    if (serie.barGap != 0)
                    {
                        gap = serie.barGap;
                    }
                }
            }
            return gap;
        }


        private HashSet<string> barStackSet = new HashSet<string>();
        private float GetBarTotalWidth(float categoryWidth, float gap)
        {
            float total = 0;
            float lastGap = 0;
            barStackSet.Clear();
            for (int i = 0; i < m_Series.Count; i++)
            {
                var serie = m_Series.series[i];
                if (serie.type == SerieType.Bar && serie.show)
                {
                    if (!string.IsNullOrEmpty(serie.stack))
                    {
                        if (barStackSet.Contains(serie.stack)) continue;
                        barStackSet.Add(serie.stack);
                    }
                    var width = GetStackBarWidth(categoryWidth, serie);
                    if (gap == -1)
                    {
                        if (width > total) total = width;
                    }
                    else
                    {
                        lastGap = width * gap;
                        total += width;
                        total += lastGap;
                    }
                }
            }
            if (total > 0 && gap != -1) total -= lastGap;
            return total;
        }

        private float GetStackBarWidth(float categoryWidth, Serie now)
        {
            if (string.IsNullOrEmpty(now.stack)) return now.GetBarWidth(categoryWidth);
            float barWidth = 0;
            for (int i = 0; i < m_Series.Count; i++)
            {
                var serie = m_Series.series[i];
                if (serie.type == SerieType.Bar && serie.show && now.stack.Equals(serie.stack))
                {
                    if (serie.barWidth > barWidth) barWidth = serie.barWidth;
                }
            }
            if (barWidth > 1) return barWidth;
            else return barWidth * categoryWidth;
        }

        protected override void DrawChart(VertexHelper vh)
        {
            base.DrawChart(vh);
            bool yCategory = m_YAxises[0].IsCategory() || m_YAxises[1].IsCategory();
            m_Series.GetStackSeries(ref m_StackSeries);
            int seriesCount = m_StackSeries.Count;
            int serieNameCount = -1;
            m_SerieNameSet.Clear();
            m_BarLastOffset = 0;
            for (int j = 0; j < seriesCount; j++)
            {
                var serieList = m_StackSeries[j];
                m_SeriesCurrHig.Clear();
                // if (m_SeriesCurrHig.Capacity != serieList[0].dataCount)
                // {
                //     m_SeriesCurrHig.Capacity = serieList[0].dataCount;
                // }
                for (int n = 0; n < serieList.Count; n++)
                {
                    Serie serie = serieList[n];
                    serie.dataPoints.Clear();
                    if (string.IsNullOrEmpty(serie.name)) serieNameCount++;
                    else if (!m_SerieNameSet.Contains(serie.name))
                    {
                        m_SerieNameSet.Add(serie.name);
                        serieNameCount++;
                    }
                    if (yCategory) DrawYBarSerie(vh, j, seriesCount, serie, serieNameCount, ref m_SeriesCurrHig);
                    else DrawXBarSerie(vh, j, seriesCount, serie, serieNameCount, ref m_SeriesCurrHig);

                }
            }
            if (yCategory) DrawYTooltipIndicator(vh);
            else DrawXTooltipIndicator(vh);
        }

        protected override void OnRefreshLabel()
        {
            var isYAxis = m_YAxises[0].type == Axis.AxisType.Category
                || m_YAxises[1].type == Axis.AxisType.Category;
            for (int i = 0; i < m_Series.Count; i++)
            {
                var serie = m_Series.GetSerie(i);
                if (serie.type == SerieType.Bar && serie.show)
                {
                    var zeroPos = Vector3.zero;
                    if (serie.label.position == SerieLabel.Position.Bottom)
                    {
                        if (isYAxis)
                        {
                            var xAxis = m_XAxises[serie.axisIndex];
                            zeroPos = new Vector3(coordinateX + xAxis.zeroXOffset, coordinateY);
                        }
                        else
                        {
                            var yAxis = m_YAxises[serie.axisIndex];
                            zeroPos = new Vector3(coordinateX, coordinateY + yAxis.zeroYOffset);
                        }
                    }
                    for (int j = 0; j < serie.data.Count; j++)
                    {
                        var serieData = serie.data[j];
                        if (serie.label.show)
                        {
                            var pos = serie.dataPoints[j];
                            switch (serie.label.position)
                            {
                                case SerieLabel.Position.Center:
                                    pos = new Vector3(pos.x, pos.y / 2);
                                    break;
                                case SerieLabel.Position.Bottom:
                                    pos = new Vector3(pos.x, zeroPos.y);
                                    break;
                            }
                            var value = serieData.data[1];
                            serieData.SetLabelActive(true);
                            serieData.SetLabelText(ChartCached.FloatToStr(value));
                            if (isYAxis)
                            {
                                if (value >= 0) serieData.SetLabelPosition(new Vector3(pos.x + serie.label.distance, pos.y));
                                else serieData.SetLabelPosition(new Vector3(pos.x - serie.label.distance, pos.y));
                            }
                            else
                            {
                                if (value >= 0) serieData.SetLabelPosition(new Vector3(pos.x, pos.y + serie.label.distance));
                                else serieData.SetLabelPosition(new Vector3(pos.x, pos.y - serie.label.distance));
                            }
                        }
                        else
                        {
                            serieData.SetLabelActive(false);
                        }
                    }
                }
            }
        }
    }


}